#Library#

## Import ##
file_path = file.choose()
raw.data = read.csv(file_path)

## P-value vector and ranking ##
ordered.data = raw.data[order(raw.data$p), ]
num.tests = nrow(ordered.data)
ordered.data$Rank = seq(1,num.tests,1)
ordered.data$lmQ = NA
false.discovery.rate = 0.25

ordered.data.total = ordered.data

ordered.data.5CSRTT = ordered.data[which(ordered.data$Task == '5CSRTT'), ]
ordered.data.5CSRTT = ordered.data.5CSRTT[order(ordered.data.5CSRTT$p), ]
ordered.data.5CSRTT$Rank = seq(1,18,1)

ordered.data.PD = ordered.data[which(ordered.data$Task == 'PD'), ]
ordered.data.PD = ordered.data.PD[order(ordered.data.PD$p), ]
ordered.data.PD$Rank = seq(1,18,1)

ordered.data.PAL = ordered.data[which(ordered.data$Task == 'PAL'), ]
ordered.data.PAL = ordered.data.PAL[order(ordered.data.PAL$p), ]
ordered.data.PAL$Rank = seq(1,12,1)

for(a in 1:num.tests){
  ordered.data.total[a,7] = (ordered.data.total[a,6] / 48) * false.discovery.rate
}
for(a in 1:18){
  ordered.data.5CSRTT[a,7] = (ordered.data.5CSRTT[a,6] / 18) * false.discovery.rate
}
for(a in 1:18){
  ordered.data.PD[a,7] = (ordered.data.PD[a,6] / 18) * false.discovery.rate
}
for(a in 1:12){
  ordered.data.PAL[a,7] = (ordered.data.PAL[a,6] / 12) * false.discovery.rate
}
