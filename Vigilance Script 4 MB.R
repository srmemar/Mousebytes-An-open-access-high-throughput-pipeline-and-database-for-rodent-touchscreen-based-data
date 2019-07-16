
Vigilance.Calc.Block3.Function = function(dataset,binsize){
  acc.start.col = which(colnames(dataset) == 'TRIAL.ANALYSIS...CORRECT._1')
  omission.start.col = which(colnames(dataset) == 'TRIAL.ANALYSIS...OMISSION._1')
  total.bins = 50 / binsize
  new.data = as.data.frame(matrix(nrow=nrow(dataset),ncol=(total.bins * 2)))
  
  col.names = c('Accuracy','Omission')
  final.colnames = c()
  for(a in 1:length(col.names)){
    for(b in 1:total.bins){
      new.name = paste(col.names[a],b,sep=' ')
      final.colnames = c(final.colnames,new.name)
    }
  }
  colnames(new.data) = final.colnames
  bin.num = 1
  final.col.acc.start = 1
  final.col.omission.start = which(colnames(new.data) == 'Omission 1')
  
  for(a in 1:nrow(dataset)){
    curr.bin = 1
    correct.vec = c()
    total.vec = c()
    omission.vec = c()
    final.acc.vec = c()
    final.omission.vec = c()
    for(b in 1:total.bins){
      correct.vec = c(correct.vec,0)
      total.vec = c(total.vec,0)
      omission.vec = c(omission.vec,0)
      final.acc.vec = c(final.acc.vec,0)
      final.omission.vec = c(final.omission.vec,0)
    }
    total.trials = as.numeric(as.character(dataset[a,14]))
    if(total.trials < 1){
      next
    }
    for(b in 1:total.trials){
      curr.mod = b-1
      
      curr.acc = as.character(dataset[a,(acc.start.col + curr.mod)])
      curr.omission = as.character(dataset[a,(omission.start.col + curr.mod)])
      if(is.na(curr.acc)){
        next
      }
      if(curr.acc == 'null'){
        next
      }
      curr.acc = as.numeric(curr.acc)
      curr.omission = as.numeric(curr.omission)
      
      if(curr.acc == 1){
        correct.vec[curr.bin] = correct.vec[curr.bin] + 1
      }else if(curr.acc == 0){
        if(curr.omission == 1){
          omission.vec[curr.bin] = omission.vec[curr.bin] + 1
        }
      }
      total.vec[curr.bin] = total.vec[curr.bin] + 1
      if((b%%binsize) == 0){
        curr.bin = curr.bin + 1
      }
      for(b in 1:total.bins){
        final.acc.vec[b] = (correct.vec[b] / (total.vec[b] - omission.vec[b])) * 100
        final.omission.vec[b] = ((omission.vec[b]) / (total.vec[b])) * 100
        new.data[a,(final.col.acc.start + b - 1)] = final.acc.vec[b]
        new.data[a,(final.col.omission.start + b - 1)] = final.omission.vec[b]
      }
    }
  }
  final.data = cbind(dataset[ ,1:14],new.data)
  return(final.data)
}

# Separate Into Strains for Analysis #
Strain.Separation.Function = function(dataset){
  new.data = list()

  new.data$APP = dataset[which(dataset$Strain=="APP/PS1"), c(1:5,9,12,15:24)]
  new.data$TG3x = dataset[which(dataset$Strain=="3xTG-AD"), c(1:5,9,12,15:24)]
  new.data$TG5x = dataset[which(dataset$Strain=="5XFAD"), c(1:5,9,12,15:24)]
  
  return(new.data)
}

# Separate Into Separate Measures for Each File + Transform # #
Measure.Separation.Function = function(dataset, long.form=0){
  new.data = list()

  new.data$APP$Accuracy = dataset$APP[ ,c(1:7,8:12)]
  new.data$APP$Omission = dataset$APP[ ,c(1:7,13:17)]

  new.data$TG5x$Accuracy = dataset$TG5x[ ,c(1:7,8:12)]
  new.data$TG5x$Omission = dataset$TG5x[ ,c(1:7,13:17)]
  
  new.data$TG3x$Accuracy = dataset$TG3x[ ,c(1:7,8:12)]
  new.data$TG3x$Omission = dataset$TG3x[ ,c(1:7,13:17)]
  return(new.data)
}

# Data Format - Long to Wide ##
Data.Formatting.Function = function(dataset){
  for(a in 1:3){
    for(b in 1:2){
      temp.data = as.data.frame(dataset[[a]][[b]])
      temp.data = temp.data[ ,c(1:4,6,8:12)]
      melt.data = melt(temp.data,id=c('AnimalID','Age','Sex','GenoType','Stimulus_Duration'))
      data.cast = dcast(melt.data, AnimalID + GenoType + Sex ~ Age + Stimulus_Duration + variable, fun.aggregate = mean, na.rm=TRUE, value.var="value")

      for(c in 6:ncol(data.cast)){
        colnames(data.cast)[c] = paste('Data',colnames(data.cast)[c],sep=".")
      }
      dataset[[a]][[b]] = as.data.frame(data.cast)
    }
  }
  return(dataset)
}


# Generate iData for Repeated Measure Design (Probe Only) #
iData.Generate.Function = function(dataset){
  template.data = as.data.frame(dataset[[1]][[1]])
  template.data = template.data[ ,c(1:4,6,8:12)]
  melt.data = melt(template.data,id=c('AnimalID','Age','Sex','GenoType','Stimulus_Duration'))
  idata = unique(melt.data[c('Stimulus_Duration','variable')])
  idata = idata[order(idata$Stimulus_Duration,idata$variable), ]
  idata[ ,2] = as.character(idata[ ,2])
  for(a in 1:nrow(idata)){
    idata[a,2] = gsub('Accuracy','',as.character(idata[a,2]))
  }
  idata$Stimulus_Duration = as.factor(idata$Stimulus_Duration)
  idata$variable = as.factor(idata$variable)
  return(idata)
}

# Calculate ANOVA Results #
Anova.Preparation.Function = function(dataset,idata){
  measure.list = list(c(24:43),c(44:63),c(4:23))
  measure.name = c('M3_6','M7_10','M11_13')
  sex.list = c('f','m')
  final.dataset = list(APP=list(M3_6=list(Female=list(Accuracy=list(),Omission=list()),Male=list(Accuracy=list(),Omission=list())),M7_10=list(Female=list(Accuracy=list(),Omission=list()),Male=list(Accuracy=list(),Omission=list())),M11_13=list(Female=list(Accuracy=list(),Omission=list()),Male=list(Accuracy=list(),Omission=list())))
                       ,TG5x=list(M3_6=list(Female=list(Accuracy=list(),Omission=list()),Male=list(Accuracy=list(),Omission=list())),M7_10=list(Female=list(Accuracy=list(),Omission=list()),Male=list(Accuracy=list(),Omission=list())),M11_13=list(Female=list(Accuracy=list(),Omission=list()),Male=list(Accuracy=list(),Omission=list())))
                       ,TG3x=list(M3_6=list(Female=list(Accuracy=list(),Omission=list()),Male=list(Accuracy=list(),Omission=list())),M7_10=list(Female=list(Accuracy=list(),Omission=list()),Male=list(Accuracy=list(),Omission=list())),M11_13=list(Female=list(Accuracy=list(),Omission=list()),Male=list(Accuracy=list(),Omission=list()))))
  for(a in 1:length(dataset)){
    for(b in 1:length(dataset[[a]])){
      data.file =dataset[[a]][[b]]
      data.file = data.file[complete.cases(data.file), ]
      for(c in 1:3){
        data.working = data.file[ ,c(1:3,measure.list[[c]])]
        for(d in 1:2){
          data.sex = data.working[which(data.working$Sex == sex.list[d]), ]
          data.sex$Sex = NULL
          data.depend = data.sex[ ,3:ncol(data.sex)]
          data.lm = lm(as.matrix(data.depend) ~ 1+ GenoType, data=data.sex)
          data.anova = Anova(data.lm, idata=idata,idesign=~Stimulus_Duration*variable, type="III")
          final.dataset[[a]][[c]][[d]][[b]] = summary(data.anova,multivariate=FALSE)
        }

      }
    }
  }
  return(final.dataset)
}

## Library ##
library(reshape2)
library(car)


raw.filepath = file.choose()
raw.data=read.csv(raw.filepath)
binsize=5

vig.data = Vigilance.Calc.Block3.Function(raw.data,10)
vig.sep.strain = Strain.Separation.Function(vig.data)
vig.sep.measure = Measure.Separation.Function(vig.sep.strain)
vig.reshape.data = Data.Formatting.Function(vig.sep.measure)
vig.idata = iData.Generate.Function(vig.sep.measure)
vig.anova = Anova.Preparation.Function(vig.reshape.data,vig.idata)

## Summary ##
strain.list = as.vector(names(vig.anova))
age.list = as.vector(names(vig.anova$APP))
sex.list = as.vector(names(vig.anova$APP$M3_6))
measure.list = as.vector(names(vig.anova$APP$M3_6$Female))
template.file = vig.anova$APP$M3_6$Female$Accuracy
template.rownames = rownames(template.file[[4]])
template.rownames = template.rownames[2:length(template.rownames)]
template.pvalads = rownames(template.file[[5]])

hm.rownames = template.rownames
hm.rownames = gsub('GenoType','Genotype',hm.rownames)
hm.rownames = gsub('Stimulus_Duration','Stimulus Length',hm.rownames)
hm.rownames = gsub('variable','Block',hm.rownames)
hm.rownames = gsub(':','*',hm.rownames)

strain.count = length(strain.list)
age.count = length(age.list)
sex.count = length(sex.list)
measure.count = length(measure.list)
analysis.count = length(hm.rownames)

map.list = list()
for(a in 1:length(strain.list)){
  summary.table = as.data.frame(matrix(nrow=(analysis.count*measure.count),ncol=9))
  colnames(summary.table) = c('Age','Sex','Measure', 'Analysis','df1','df2','F','p','partial eta^2')
  row.modifier = 1
  for(b in 1:length(age.list)){
    for(f in 1:length(sex.list)){
      for(d in 1:length(measure.list)){
        temp.summary = vig.anova[[strain.list[a]]][[age.list[b]]][[sex.list[f]]][[measure.list[d]]]
        temp.main = temp.summary[[4]]
        temp.pvalad = temp.summary[[5]]
        for(c in 1:length(template.pvalads)){
          temp.main[which(rownames(temp.main) == template.pvalads[c]),6] = temp.pvalad[which(rownames(temp.pvalad) == template.pvalads[c]),2]
        }
        for(c in 1:length(hm.rownames)){
          partial.eta = temp.main[(c+1),1] / (temp.main[(c+1),1] + temp.main[(c+1),3])
          summary.table[row.modifier,1] = age.list[b]
          summary.table[row.modifier,2] = sex.list[f]
          summary.table[row.modifier,3] = measure.list[d]
          summary.table[row.modifier,4] = hm.rownames[c]
          summary.table[row.modifier,5] = temp.main[(c+1),2]
          summary.table[row.modifier,6] = temp.main[(c+1),4]
          summary.table[row.modifier,7] = round(temp.main[(c+1),5],digits=2)
          summary.table[row.modifier,8] = round(temp.main[(c+1),6],digits=3)
          summary.table[row.modifier,9] = round(partial.eta, digits = 2)
          row.modifier = row.modifier + 1
        }
      }
    }
  }

  map.list[[strain.list[a]]] = summary.table
}

