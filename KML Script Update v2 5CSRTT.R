## Library ##
library(dplyr)
library(kml3d)
library(reshape2)
library(ggplot2)

## Functions ##
Data.Formatting.Function.5CSRTT = function(dataset){
  dataset$StimulusLength = as.factor(dataset$StimulusLength)
  dataset$StimulusLength = relevel(dataset$StimulusLength,'0.8')
  dataset$StimulusLength = relevel(dataset$StimulusLength,'1')
  dataset$StimulusLength = relevel(dataset$StimulusLength,'1.5')
  melt.data = melt(dataset,id.vars=c(1:7))
  melt.data[ ,1] = as.character(melt.data[ ,1])
  melt.data[ ,6] = as.character(melt.data[ ,6])
  for(a in 1:nrow(melt.data)){
    melt.data[a,1] = paste(as.character(melt.data[a,1]),as.character(melt.data[a,6]),sep='.')
  }
  cast.data = dcast(melt.data,Animal.ID + TestSite + MouseStrain + Genotype + Sex + Age ~ variable + StimulusLength, fun.aggregate = mean, na.rm=TRUE)
  return(cast.data)
}

Fisher.Analysis.Function = function(dataset){
  strain.list = list(c('B6129SF2/J','3xTG-AD'),c('B6SJLF1/J','5xFAD'),c('C57BL6','APPPS1'))
  sex.list = as.vector(unique(dataset$Sex))
  age.list = sort(as.vector(unique(as.numeric(as.character(dataset$Age)))))
  fisher.table = as.data.frame(matrix(nrow=3,ncol=3))
  colnames(fisher.table) = c('3xTG','5xFAD','APP/PS1')
  rownames(fisher.table) = c('4Months', '7 Months', '10 Months')
  fisher.table.female = fisher.table
  fisher.table.male = fisher.table

  for(a in 1:length(strain.list)){
    for(b in 1:length(sex.list)){
      for(c in 1:length(age.list)){
        test.data = dataset[which(((dataset$Strain == strain.list[[a]][1]) | (dataset$Strain == strain.list[[a]][2])) & (dataset$Sex == sex.list[b]) & (dataset$Age == age.list[c])), ]
        test.cont.data = as.data.frame(matrix(nrow=2,ncol=3))
        colnames(test.cont.data) = c('High', 'Mid', 'Low')
        rownames(test.cont.data) = c(strain.list[[a]][1], strain.list[[a]][2])
        for(d in 1:2){
          for(e in 1:3){
            if(isTRUE(length(test.data[which((test.data$Strain == rownames(test.cont.data)[d]) & (test.data$Cluster == colnames(test.cont.data)[e])), 5]) == 0)){
              test.cont.data[d,e] = 0
            }else{
              test.cont.data[d,e] = test.data[which((test.data$Strain == rownames(test.cont.data)[d]) & (test.data$Cluster == colnames(test.cont.data)[e])), 5] 
            }
          }
        }
        test.cont.data[is.na(test.cont.data)] = 0
        fisher.result = fisher.test(test.cont.data)
        
        if(sex.list[b] == 'Female'){
        fisher.table.female[c,a] = fisher.result$p.value
        }
        if(sex.list[b] == 'Male')
        fisher.table.male[c,a] = fisher.result$p.value
      }
    }
  }
  fisher.list= list()
  fisher.list$Female = fisher.table.female
  fisher.list$Male = fisher.table.male
  return(fisher.list)
}

## Read Data ##
raw.data.path = file.choose()
raw.data.probe = read.csv(raw.data.path)

raw.data.probe = raw.data.probe[ ,c(3,4,5,6,7,8,9,16,17,18,19,122,70)]
colnames(raw.data.probe) = c('Animal.ID','TestSite','MouseStrain','Genotype','Sex','Age','StimulusLength','Accuracy','Omissions','Premature','Perseverative','CorrectLatency','RewardLatency')

for(a in 8:13){
  raw.data.probe[,a] = scale(raw.data.probe[,a])
}


formatted.data.probe = Data.Formatting.Function.5CSRTT(raw.data.probe)

formatted.data.probe = na.omit(formatted.data.probe) #Omit Data


## Set Parameters ##
kml.group.no = 3

## Rum K-Mean Algorithm ##

kma.3d.analysis = cld3d(formatted.data.probe, timeInData = list(Accuracy=c(7:10)
                                                                ,Omission=c(11:14)
                                                                ,Premature=c(15:18)
                                                                ,Perseverative=c(19:22)
                                                                ,CorrectLatency=c(23:26)
                                                                ,RewardLatency=c(27:30)))

kml3d(kma.3d.analysis)

combined.data = formatted.data.probe

combined.data$Cluster = getClusters(kma.3d.analysis,kml.group.no)

## Save Count ##
count.data = combined.data
count.data$Count = 1
count.data = aggregate(count.data$Count,by=list(count.data$Cluster,count.data$Genotype,count.data$Sex,count.data$Age),FUN=sum, na.rm=TRUE)
colnames(count.data) = c('Cluster','Strain','Sex','Age','Count')
count.data$Cluster = recode(count.data$Cluster,'A' = 'Mid','B' = 'Low', 'C' = 'High')

fisher.data = Fisher.Analysis.Function(count.data)

heatmap.2(as.matrix(fisher.data$Male)
          ,density.info = 'none',trace='none',col=color.pallete, breaks = color.breaks
          ,dendrogram='none',Colv='NA',Rowv=FALSE,srtCol=45,key=FALSE
          ,lwid=c(0.1,80), lhei=c(0.1,8) ,cexCol = 1.5,cexRow=1.5,offsetRow = 0
          ,offsetCol = 0,margins=c(0,0),labRow = NULL,labCol = NULL
          ,cellnote = format(round(fisher.data$Male,digits=2), nsmall=2),notecex = 2.0,notecol = 'black')
## Visualize
graphing.data.mean = aggregate(combined.data[ ,7:30],by=list(combined.data$Cluster), FUN=mean, na.rm=TRUE)

graphing.melt.data = melt(graphing.data.mean)

new.group.data = as.data.frame(stringr::str_split_fixed(graphing.melt.data$variable,'_',2))

day.vector = as.vector(new.group.data[ ,2])
day.unique = as.vector(unique(day.vector))

for(a in length(day.unique):1){
  day.vector[day.vector==day.unique[a]] = a
}

graphing.melt.data[ ,2] = new.group.data[ ,1]
graphing.melt.data$Time = day.vector

graphing.data.cast = dcast(graphing.melt.data, Group.1 + Time ~ variable, fun.aggregate = mean, na.rm=TRUE)
graphing.data.cast = graphing.data.cast[order(graphing.data.cast$Group.1,as.numeric(graphing.data.cast$Time)), ]

graphing.data.cast.4m = graphing.data.cast[c(1:4,13:16,25:28), ]
graphing.data.cast.4m$Time = dplyr::recode_factor(graphing.data.cast.4m$Time, '4'='0.6', '3'='0.8', '2'='1', '1'='1.5')
graphing.data.cast.4m$Time = factor(graphing.data.cast.4m$Time,levels=graphing.data.cast.4m$Time[c(4,3,2,1)])
graphing.data.cast.7m = graphing.data.cast[c(5:8,17:20,29:32), ]
graphing.data.cast.7m$Time = dplyr::recode_factor(graphing.data.cast.7m$Time, '8'='0.6', '7'='0.8', '6'='1', '5'='1.5')
graphing.data.cast.7m$Time = factor(graphing.data.cast.7m$Time,levels=graphing.data.cast.7m$Time[c(4,3,2,1)])
graphing.data.cast.10m = graphing.data.cast[c(9:12,21:24,33:36), ]
graphing.data.cast.10m$Time = dplyr::recode_factor(graphing.data.cast.10m$Time, '12'='0.6', '11'='0.8', '10'='1', '9'='1.5')
graphing.data.cast.10m$Time = factor(graphing.data.cast.10m$Time,levels=graphing.data.cast.10m$Time[c(4,3,2,1)])

ggplot(data=graphing.data.cast.4m,aes(x=graphing.data.cast$Time,y=graphing.data.cast$Accuracy,group=as.factor(graphing.data.cast$Group.1))) + 
  geom_line(aes(color=as.factor(graphing.data.cast$Group.1))) + 
  geom_point(aes(color=as.factor(graphing.data.cast$Group.1))) +
  scale_x_discrete(limits = rev(levels(graphing.data.cast$Time)))
