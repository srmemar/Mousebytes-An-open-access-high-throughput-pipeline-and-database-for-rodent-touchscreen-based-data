# MouseBytes Quality Control Code
Quality control (QC) procedure in MouseBytes checks the content of xml files against the potential errors and flag the files that do not meet the criteria (refer to "QC Documents" folder). Such xml files belong to a cognitive task experimented using Bussey-Saksida Mouse Touchscreen System and exported via ABET II software. Note that each xml file must contain the machine-generated (ABET II) features like "Analysis Name", "Schedule Name", "Max_Number_Trials", and "Max_Schedule_Time". 


MouseBytes is a web-based application connected to the database (Microsoft SQL server) which is protected by copy-right to avoid commercialization of the proposed application by the third party. In this version, we provided the windows-based application written in c#.net that only checks the quality control. (please refer to "MB_QualityControl" folder for all project files or run MB_QaulityControl.sln). 
When you run the code, the application asks the user to enter the address of the directory that hosts all the xml files for a cognitive task, and the address of the directory where you wish to save the output file (result of QC) in your pc. 



