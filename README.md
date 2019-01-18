# MouseBytes-Quality-Control
The following code checks the content of data files (xml) belong to a cognitive task experimented using Bussey-Saksida Mouse Touchscreen System and exported via ABET II software and flags those files that do not meet the QC (quality control) criteria. 

Our code currently performs the quality control checks for 5Choice, PAL (Paired Associate Learning) and PD (Pairwise Visual Discrimination). Each xml file must contain the machine (ABET II) generated features like "Analysis Name", "Schedule Name", "Max_Number_Trials", and "Max_Schedule_Time". 

When you run the QC code, it  first asks the address of the directory where all your xml files hosted in your computer/server. Based on the value of "Analysis Name", it is determined what the cognitive task is, and the corresponding QC rules are checked for that file. Once all the files in the directory are checked, a txt file is generated as an output showing the report of QC.
