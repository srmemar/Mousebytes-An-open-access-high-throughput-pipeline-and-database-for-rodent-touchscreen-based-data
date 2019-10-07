# MouseBytes Quality Control Code

Quality control (QC) procedure in MouseBytes checks the content of xml files against the potential errors and flag the files that do not meet the criteria (refer to "QC Documents" folder). Such xml files belong to a cognitive task experimented using Bussey-Saksida Mouse Touchscreen System and exported via ABET II software. Note that each xml file must contain the machine-generated (ABET II) features like "Analysis Name", "Schedule Name", "Max_Number_Trials", and "Max_Schedule_Time". 


MouseBytes is a web-based application connected to the database (Microsoft SQL server) which is protected by copy-right to avoid commercialization of the proposed application by the third party. In this version, we provided the windows-based application written in c#.net that only checks the quality control. (please refer to "MB_QualityControl" folder for all project files or run MB_QaulityControl.sln). 
When you run the code, the application asks the user to enter the address of the directory that hosts all the xml files for a cognitive task, and the address of the directory where you wish to save the output file (result of QC) in your pc. 

# R scripts for statistical analysis of cognitive behavioral data

### Summary Table Scripts (Summary Table 5CSRTTv2.R, Summary Table PD v2.R, Summary Table PAL v2.r)

These scripts are designed to reformat the raw data files for usage in split-plot ANOVA. The initial functions of these scripts filter the data into the respective genotypes and convert the data organization. Following that, the script will calculate the appropriate within measures data frame and run split plot ANOVA analyses on the data. In order to operate these scripts, one only needs to change the file path for the data files (pretraining and test data) and execute the entire script.

-----------------------------------------------------------------------------
### Benjamini Hochburg FDR Fisher Correction.R

In this script the calculated p values for the Fisher’s exact tests are corrected using the Benjamini Hochburg False Discovery Rate correction factor. To operate this script, one must run the script, and select the file with p-values from the file path.

-----------------------------------------------------------------------------
### KML Script Update (KML Script Update v2 PAL.R, KML Script Update v2 PD.R, KML Script Update v2 5CSRTT.R)

In these scripts, data from the touchscreen experiments is run through a K-Mean clustering package (KML3D) to generate k-mean groupings from our data. In addition to this process, this script also calculates data tables with p-values associated with the Fisher’s Exact Test. To operate this script, run the entire script and select the appropriate data file from the file folder list.

-----------------------------------------------------------------------------
### Vigilance Script v4 MB.R

This script conducts additional analyses on the 5-CSRTT dataset to generate the vigilance data. Trial-by-trial data is segmented and binned based on blocks of 10 trials. Once this process is complete, the script conducts split-plot ANOVA analyses on the generated datasets. In order to use this script, run the entire script and select the raw data file in the file prompt.
-----------------------------------------------------------------------------
# Integrating external behaviour systems with MouseBytes
### XML_Output.ipynb
It is jupyter a notebook file (python 3). You can find guidelines, documentations, and cells of codes in this file. This python scripts enables other systems to generate the output in xml foramt which contains the key features required for analysis of each cognitive task.
