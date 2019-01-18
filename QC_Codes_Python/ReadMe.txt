Abet II analysis files are exported in XML format. All the exported XML files are hosted in the server at Robarts Research Institute. The quality control codes check the content of each xml file and compares them with some conditions (Quality Control Rules). For example, the quality control code for 5_choice task generates three output text files as follows and emails them to the user:

1-	flagged5C.txt: Includes list of flagged xml files for 5_choice task.
2-	complete_sessions5C.txt:   Includes list of all sessions for the task (e.g. 5_choice), the number of xml files/Analysis each session should have, and the name of file(s) for the corresponding session.
3-	incomplete_sessions5C.txt: Includes list of all sessions for the task (e.g. 5_choice), the expected number of xml files/Analysis each session should have, the actual number of xml files each session has, and the name of file(s) for the corresponding session.

Quality control codes were provided for the following tasks and put in the corresponding folder:

1-	5_choice (QC_5C)
2-	Paired Associate Learning (QC_PAL)
3-	Pairwise Discrimination (QC_PD)

The main file in each folder that should be first executed is “fileQC.py”. This file (i.e. fileQC.py) reads the input config file named “fileQC.conf”. Note that the path of config file (line 396 in fileQC.py) should be changed based on the path in your machine.

Config file (i.e. fileQC.conf) includes the following tags and you may need to change some of the information in this file: 

1-	<email>: The output files of the code are emailed to the user. Email addresses of the sender and receiver, subject and body of email are set in this tag. 
2-	<rawFolder> : Path of the folder that includes the input raw xml files is set in this tag.
3-	<ignoredFolder>: Path of the folder that includes the xml files which should be ignored. (this folder is initially empty)
4-	<junkFolder>: Path of the folder that includes the xml files already checked and flagged as error. (this folder is initially empty)
5-	<IDlists> : This tag includes the name of the excel sheet. The excel sheet includes the name of databases (e.g.  LON5C3XTG, LON5C5FAD and LON5CAPP for 5_Choice task) and the corresponding animal IDs.
6-	<flaggedFile>, <completedFile>, and <incompletedFile> tags include the paths of three output text files explained above.

The quality control rules are included in the following files in each folder/for each task:

1-	qc.xml
2-	qc_condition.xml
3-	qc_session_count.xml
