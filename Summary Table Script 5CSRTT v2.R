#################################################
#                                               #
#                                               #
#         Weston Anova Script                   #
#             5-CSRTT                           #
#                                               #
#                                               #
#################################################

# These scripts are designed to reformat 5CSRTT raw data files for usage in split-plot ANOVA. The initial functions of these scripts filter the data into the respective genotypes and convert the data organization.
# Copyright (C) 2019 Daniel Palmer
# Please see LICENSE.md file for Copyright: https://github.com/srmemar/Mousebytes-An-open-access-high-throughput-pipeline-and-database-for-rodent-touchscreen-based-data/blob/master/LICENSE

## Library ##
library(tidyverse)
library(reshape2)
library(car)

library(gplots)
library(RColorBrewer)

## Functions List ##

# Separate Into Strains for Analysis #
Strain.Separation.Function = function(dataset,long.form=0){
  new.data = list()
  if(long.form == 0){
    new.data$APP = dataset[which(dataset$Mouse.Strain=="APP-PS1"), ]
    new.data$TG3x = dataset[which(dataset$Mouse.Strain=="3xTG-AD"), ]
    new.data$TG5x = dataset[which(dataset$Mouse.Strain=="5xFAD"), ] 
  }else if(long.form == 1){
    new.data$APP = dataset[which(dataset$Mouse.Strain=="APP-PS1"), c(2:9,14:19,70,122)]
    new.data$TG3x = dataset[which(dataset$Mouse.Strain=="3xTG-AD"), c(2:9,14:19,70,122)]
    new.data$TG5x = dataset[which(dataset$Mouse.Strain=="5xFAD"), c(2:9,14:19,70,122)]
  }
  return(new.data)
}

# Separate Into Separate Measures for Each File + Transform # #
Measure.Separation.Function = function(dataset, long.form=0){
  new.data = list()
  if(long.form == 0){
    new.data$APP$Sessions = dataset$APP[ ,c(2:9)]
    new.data$TG5x$Sessions = dataset$TG5x[ ,c(2:9)]
    new.data$TG3x$Sessions = dataset$TG3x[ ,c(2:9)]
  }else if(long.form == 1){
    new.data$APP$TotalTime = dataset$APP[ ,c(2:8,9)]
    new.data$APP$TotalTrials = dataset$APP[ ,c(2:8,10)]
    new.data$APP$Accuracy = dataset$APP[ ,c(2:8,11)]
    new.data$APP$Omission = dataset$APP[ ,c(2:8,12)]
    new.data$APP$Premature = dataset$APP[ ,c(2:8,13)]
    new.data$APP$Perseverative = dataset$APP[ ,c(2:8,14)]
    new.data$APP$RewardLat = dataset$APP[ ,c(2:8,15)]
    new.data$APP$CorrectLat = dataset$APP[ ,c(2:8,16)]
    
    new.data$TG5x$TotalTime = dataset$TG5x[ ,c(2:8,9)]
    new.data$TG5x$TotalTrials = dataset$TG5x[ ,c(2:8,10)]
    new.data$TG5x$Accuracy = dataset$TG5x[ ,c(2:8,11)]
    new.data$TG5x$Omission = dataset$TG5x[ ,c(2:8,12)]
    new.data$TG5x$Premature = dataset$TG5x[ ,c(2:8,13)]
    new.data$TG5x$Perseverative = dataset$TG5x[ ,c(2:8,14)]
    new.data$TG5x$RewardLat = dataset$TG5x[ ,c(2:8,15)]
    new.data$TG5x$CorrectLat = dataset$TG5x[ ,c(2:8,16)]
    
    new.data$TG3x$TotalTime = dataset$TG3x[ ,c(2:8,9)]
    new.data$TG3x$TotalTrials = dataset$TG3x[ ,c(2:8,10)]
    new.data$TG3x$Accuracy = dataset$TG3x[ ,c(2:8,11)]
    new.data$TG3x$Omission = dataset$TG3x[ ,c(2:8,12)]
    new.data$TG3x$Premature = dataset$TG3x[ ,c(2:8,13)]
    new.data$TG3x$Perseverative = dataset$TG3x[ ,c(2:8,14)]
    new.data$TG3x$RewardLat = dataset$TG3x[ ,c(2:8,15)]
    new.data$TG3x$CorrectLat = dataset$TG3x[ ,c(2:8,16)]
  }
  return(new.data)
}

# Data Format - Long to Wide ##
Data.Formatting.Function = function(dataset,m.value,datatype){
  for(a in 1:3){
    for(b in 1:m.value){
      temp.data = as.data.frame(dataset[[a]][[b]])
      colnames(temp.data)[8] = 'Value'
      if(isTRUE(datatype == 0)){
        data.cast = dcast(temp.data, AnimalID + TestSite + Mouse.Strain + Genotype + Sex ~ Age.Months + Task, fun.aggregate = mean, na.rm=TRUE, value.var="Value")
      }else if(isTRUE(datatype == 1)){
        data.cast = dcast(temp.data, AnimalID + TestSite + Mouse.Strain + Genotype + Sex ~ Age.Months + Stimulus.Length, fun.aggregate = mean, na.rm=TRUE, value.var="Value")
      }
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
  template.data = dataset[[1]][[1]]
  idata = unique(template.data[c('Age.Months','Stimulus.Length')])
  idata = idata[order(idata$Age.Months,idata$Stimulus.Length), ]
  idata$Age.Months = as.factor(idata$Age.Months)
  idata$Stimulus.Length = as.factor(idata$Stimulus.Length)
  return(idata)
}

# Calculate ANOVA Results #
Anova.Preparation.Probe.Function = function(dataset,idata){
  for(a in 1:length(dataset)){
    for(b in 1:length(dataset[[a]])){
      data.file =dataset[[a]][[b]]
      data.file = data.file[complete.cases(data.file), ]
      data.file[ ,c(1,3)] = NULL
      data.depend = data.file[ ,4:ncol(data.file)]
      data.lm = lm(as.matrix(data.depend) ~ 1+ TestSite * Genotype * Sex, data=data.file)
      data.anova = Anova(data.lm, idata=idata,idesign=~Age.Months*Stimulus.Length, type="III")
      dataset[[a]][[b]] = data.anova
    }
  }
  return(dataset)
}

Anova.Preparation.Pretrain.Function = function(dataset){
  final.dataset = list()
  for(a in 1:length(dataset)){
    for(b in 1:length(dataset[[a]])){
      data.file =dataset[[a]][[b]]
      data.file = data.file[complete.cases(data.file), ]
      data.file[ ,c(1,3)] = NULL
      data.depend = data.file[ ,7]
      data.lm = lm(as.matrix(data.depend) ~ 1+ TestSite * Genotype * Sex, data=data.file)
      data.anova = Anova(data.lm, type="III")
      dataset[[a]][[b]] = data.anova
    }
  }
  return(dataset)
}

Anova.Preparation.Acquisition.Function = function(dataset,trainingstim){
  final.dataset = list()
  for(a in 1:length(dataset)){
    for(b in 1:length(dataset[[a]])){
      data.file =dataset[[a]][[b]]
      data.file = data.file[complete.cases(data.file), ]
      data.file[ ,c(1,3)] = NULL
      if(trainingstim == 4){
        data.depend = data.file[ ,5]
      }else if(trainingstim == 2){
        data.depend = data.file[ ,4]
      }
      data.lm = lm(as.matrix(data.depend) ~ 1+ TestSite * Genotype * Sex, data=data.file)
      data.anova = Anova(data.lm, type="III")
      dataset[[a]][[b]] = data.anova
    }
  }
  return(dataset)
}

## Settings ##
options(scipen=50)
options(contrasts = c('contr.sum','contr.poly'))

## Read Data ##

raw.data.pretrain = read.csv('C:\\Users\\dpalmer\\Documents\\Weston_R_Script\\Data\\Raw\\5CSRTT\\Weston 5CSRTT Pretrain QC Mar 26 2018.csv')
raw.data.acq = read.csv('C:\\Users\\dpalmer\\Documents\\Weston_R_Script\\Data\\Raw\\5CSRTT\\Weston 5CSRTT Acquisition Aggregated QC Mar 26 2018.csv')
raw.data.probe = read.csv('C:\\Users\\dpalmer\\Documents\\Weston_R_Script\\Data\\Raw\\5CSRTT\\Weston 5CSRTT Probe Aggregated QC Mar 26 2018.csv')

## Separate each Raw File by Strain ##
pretrain.separated.data = Strain.Separation.Function(raw.data.pretrain,0)
acq.separated.data = Strain.Separation.Function(raw.data.acq,0)
probe.separated.data = Strain.Separation.Function(raw.data.probe,1)

## Separate Probe Data by Measure / Get Specific Columns ##
pretrain.separated.measures = Measure.Separation.Function(pretrain.separated.data,0)
acq.separated.measures = Measure.Separation.Function(acq.separated.data,0)
probe.separated.measures = Measure.Separation.Function(probe.separated.data,1)

## Format Data Long to Wide ##
pretrain.formatted.data = Data.Formatting.Function(pretrain.separated.measures,1,0)
acq.formatted.data = Data.Formatting.Function(acq.separated.measures,1,1)
probe.formatted.data = Data.Formatting.Function(probe.separated.measures,8,1)

## Gather iData for Repeated Measures ##
probe.idata = iData.Generate.Function(probe.separated.measures)

## Conduct ANOVA ##
pretrain.anova = Anova.Preparation.Pretrain.Function(pretrain.formatted.data)
acq.4.anova = Anova.Preparation.Acquisition.Function(acq.formatted.data,4)
acq.2.anova = Anova.Preparation.Acquisition.Function(acq.formatted.data,2)
probe.anova = Anova.Preparation.Probe.Function(probe.formatted.data,probe.idata)


## Prepare ANOVA Table ##
strain.list = as.vector(names(probe.anova))
measure.list = as.vector(names(probe.anova$APP))
measure.list = measure.list[c(3:8)]
template.file = summary(probe.anova[[1]][[1]], multivariate=FALSE)
template.rownames = rownames(template.file[[4]])
template.rownames = template.rownames[2:length(template.rownames)]
template.pvalads = rownames(template.file[[5]])

hm.rownames = template.rownames
#hm.rownames = gsub('Genotype','G',hm.rownames)
#hm.rownames = gsub('Sex','Sx',hm.rownames)
hm.rownames = gsub('TestSite','Test Site',hm.rownames)
hm.rownames = gsub('Stimulus.Length','Stimulus Length',hm.rownames)
hm.rownames = gsub('Age.Months','Age',hm.rownames)
hm.rownames = gsub(':','*',hm.rownames)

strain.count = length(strain.list)
measure.count = length(measure.list)
analysis.count = length(hm.rownames)


map.list = list()
for(a in 1:length(strain.list)){
  summary.table = as.data.frame(matrix(nrow=(analysis.count*measure.count),ncol=7))
  colnames(summary.table) = c('Measure', 'Analysis','df1','df2','F','p','partial eta^2')
  row.modifier = 1
  for(d in 1:length(measure.list)){
    temp.summary = summary(probe.anova[[strain.list[a]]][[measure.list[d]]], multivariate=FALSE)
    temp.main = temp.summary[[4]]
    temp.pvalad = temp.summary[[5]]
    for(c in 1:length(template.pvalads)){
      temp.main[which(rownames(temp.main) == template.pvalads[c]),6] = temp.pvalad[which(rownames(temp.pvalad) == template.pvalads[c]),2]
    }
    for(c in 1:length(hm.rownames)){
      partial.eta = temp.main[(c+1),1] / (temp.main[(c+1),1] + temp.main[(c+1),3])
      summary.table[row.modifier,1] = measure.list[d]
      summary.table[row.modifier,2] = hm.rownames[c]
      summary.table[row.modifier,3] = temp.main[(c+1),2]
      summary.table[row.modifier,4] = temp.main[(c+1),4]
      summary.table[row.modifier,5] = round(temp.main[(c+1),5],digits=2)
      summary.table[row.modifier,6] = round(temp.main[(c+1),6],digits=3)
      summary.table[row.modifier,7] = round(partial.eta, digits = 2)
      row.modifier = row.modifier + 1
    }
  }
  map.list[[strain.list[a]]] = summary.table
}

