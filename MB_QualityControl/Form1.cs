using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;



namespace MB_QualityControl
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // The following button checks the content of each XML file in the given directory against QC rules
        private void btn1_Click(object sender, EventArgs e)
        {
            //variable declaration
            string Analysis_Name;
            string Schedule_Name;
            string Max_Number_Trials;
            string Max_Schedule_Time;
            string Animal_ID;
            string End_Summary_Condition;
            string End_Summary_No_Images;
            string End_Summary_Corrects;
            string End_Summary_Trials_Completed;
            string Threshold_Condition;
            string Threshold_Trials;
            string Date;
            string ErrorMessage1 = "";
            string ErrmasgHeading = "";

            string dirPath = textBox1.Text;   // input path contains all subfolders/xml files
            string outputFilePath = textBox2.Text + @"\QC_Result.txt"; // output path to save the result of QC 
            XDocument xdoc;

            if(string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("You must enter input path and output path!");
                return;
            }

            // To get list of all files in the main folder and subfolder
            string[] Dir = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);

            //Cheking all the files in the subfolder
            foreach (string dir in Dir)
            {
                xdoc = XDocument.Load(dir);
                string[] tokens = dir.Split('\\');
                string filename = tokens.Last(); 
                //*****************************************SessionInformation******************************************

                //*******Analysis Name **************
                Analysis_Name = FeatureExtraction("SessionInformation", "Information", "Analysis Name", "Value", xdoc);
                //*******Schedule Name **************
                Schedule_Name = FeatureExtraction("SessionInformation", "Information", "Schedule Name", "Value", xdoc);
                //*******Max_Number_Trials***********
                Max_Number_Trials = FeatureExtraction("SessionInformation", "Information", "Max_Number_Trials", "Value", xdoc);
                //******Max_Schedule_Time***********
                Max_Schedule_Time = FeatureExtraction("SessionInformation", "Information", "Max_Schedule_Time", "Value", xdoc);
                //*********Date/Time****************
                Date = FeatureExtraction("SessionInformation", "Information", "Date/Time", "Value", xdoc);
                //******Animal ID******************
                Animal_ID = FeatureExtraction("SessionInformation", "Information", "Animal ID", "Value", xdoc);

                //*****************************************MarkerData******************************************************
                End_Summary_Condition = FeatureExtraction("MarkerData", "Marker", "End Summary - Condition", "Results", xdoc);
                //**********End Summary - No. images*******
                End_Summary_No_Images = FeatureExtraction("MarkerData", "Marker", "End Summary - No. images", "Results", xdoc);
                //*********End Summary - Corrects**********
                End_Summary_Corrects = FeatureExtraction("MarkerData", "Marker", "End Summary - Corrects", "Results", xdoc);
                //*********End Summary - Trials Completed*******
                End_Summary_Trials_Completed = FeatureExtraction("MarkerData", "Marker", "End Summary - Trials Completed", "Results", xdoc);
                //*********Threshold - Condition **************
                Threshold_Condition = FeatureExtraction("MarkerData", "Marker", "Threshold - Condition", "Results", xdoc);
                //********Threshold - Trials*******************
                Threshold_Trials = FeatureExtraction("MarkerData", "Marker", "Threshold - Trials", "Results", xdoc);

                if (string.IsNullOrEmpty(Animal_ID))
                {
                    ErrorMessage1 += "No Animal ID was found in the uploaded file <br/>";
                }

                //***************************"Habituation 1 (5C, PAL, PD, LD)"*************************

                if (Analysis_Name.Trim().ToLower().Contains("habit 1") || Schedule_Name.Trim().ToLower().Contains("habituation 1") || Schedule_Name.Trim().ToLower().Contains("habituation_1"))
                {
                    if (!(!string.IsNullOrEmpty(Analysis_Name) && Analysis_Name.Trim().ToLower().Contains("1") && Schedule_Name.Trim().ToLower().Contains("1")))
                    {
                        ErrorMessage1 += "Analysis Name does not match with Schedule Name. ";
                    }

                    (bool flag, string ErrMsg) info = Check_Habit_1(Max_Number_Trials, Max_Schedule_Time, End_Summary_Condition);


                    if (info.flag)
                    { }

                    else
                    {
                        // type a message 
                        ErrorMessage1 += info.ErrMsg;
                    }
                }

                //***************************"Habituation 2 (5C, PAL, PD, LD)"*************************

                if (Analysis_Name.Trim().ToLower().Contains("habit 2") || Schedule_Name.Trim().ToLower().Contains("habituation 2") || Schedule_Name.Trim().ToLower().Contains("habituation_2"))
                {
                    if (!(!string.IsNullOrEmpty(Analysis_Name) && Analysis_Name.Trim().ToLower().Contains("2") && Schedule_Name.Trim().ToLower().Contains("2")))
                    {
                        ErrorMessage1 += "Analysis Name does not match with Schedule Name. ";
                    }
                    (bool flag, string ErrMsg) info = Check_Habit_2(Max_Number_Trials, Max_Schedule_Time, End_Summary_Condition);


                    if (info.flag)
                    { }


                    else
                    {
                        // type a message 
                        ErrorMessage1 += info.ErrMsg;
                    }
                }

                //************************"Initial Touch" --->"Initial Train" for (5C, PAL, PD, LD)************************

                if (Analysis_Name.Trim().ToLower().Contains("initial train") || Analysis_Name.Trim().ToLower().Contains("initial_train") || Schedule_Name.Trim().ToLower().Contains("initial train") ||
                    Schedule_Name.Trim().ToLower().Contains("initial touch") || Schedule_Name.Trim().ToLower().Contains("initial_touch") || Schedule_Name.Trim().ToLower().Contains("initial_train"))
                {
                    if (!(!string.IsNullOrEmpty(Analysis_Name) && Analysis_Name.Trim().ToLower().Contains("initial") && Schedule_Name.Trim().ToLower().Contains("initial")))
                    {
                        ErrorMessage1 += "Analysis Name does not match with Schedule Name. ";
                    }
                    (bool flag, string ErrMsg) info = Check_Initial_Touch(Max_Number_Trials, Max_Schedule_Time, End_Summary_Condition, End_Summary_No_Images, End_Summary_Trials_Completed);


                    if (info.flag)
                    { }

                    else
                    {
                        // type a message 
                        ErrorMessage1 += info.ErrMsg;
                    }
                }

                //************************"Must Touch" for (5C, PAL, PD, LD)************************

                if (Analysis_Name.Trim().ToLower().Contains("must touch") || Analysis_Name.Trim().ToLower().Contains("must_touch") || Schedule_Name.Trim().ToLower().Contains("must touch") ||
                    Schedule_Name.Trim().ToLower().Contains("must_touch"))
                {
                    if (!(!string.IsNullOrEmpty(Analysis_Name) && Analysis_Name.Trim().ToLower().Contains("touch") && Schedule_Name.Trim().ToLower().Contains("touch")))
                    {
                        ErrorMessage1 += "Analysis Name does not match with Schedule Name. ";
                    }
                    (bool flag, string ErrMsg) info = Check_Must_Touch(Max_Number_Trials, Max_Schedule_Time, End_Summary_Condition, End_Summary_Corrects, End_Summary_Trials_Completed);

                    if (info.flag)
                    { }

                    else
                    {
                        // type a message 
                        ErrorMessage1 += info.ErrMsg;
                    }
                }

                //************************"Must Initiate" for (5C, PAL, PD, LD)************************

                if (Analysis_Name.Trim().ToLower().Contains("must initiate") || Analysis_Name.Trim().ToLower().Contains("must_initiate") || Schedule_Name.Trim().ToLower().Contains("must initiate") ||
                    Schedule_Name.Trim().ToLower().Contains("must_initiate"))
                {
                    if (!(!string.IsNullOrEmpty(Analysis_Name) && Analysis_Name.Trim().ToLower().Contains("initiate") && Schedule_Name.Trim().ToLower().Contains("initiate")))
                    {
                        ErrorMessage1 += "Analysis Name does not match with Schedule Name. ";
                    }
                    (bool flag, string ErrMsg) info = Check_Must_Touch(Max_Number_Trials, Max_Schedule_Time, End_Summary_Condition, End_Summary_Corrects, End_Summary_Trials_Completed);

                    if (info.flag)
                    { }

                    else
                    {
                        // type a message 
                        ErrorMessage1 += info.ErrMsg;
                    }
                }

                //**********************Punish Incorrect for (5C, PAL, PD, LD)****************

                if (Analysis_Name.Trim().ToLower().Contains("punish incor") || Schedule_Name.Trim().ToLower().Contains("punish incorrect") || Schedule_Name.Trim().ToLower().Contains("punish_incorrect"))
                {
                    if (!(!string.IsNullOrEmpty(Analysis_Name) && Analysis_Name.Trim().ToLower().Contains("punish") && Schedule_Name.Trim().ToLower().Contains("punish")))
                    {
                        ErrorMessage1 += "Analysis Name does not match with Schedule Name. ";
                    }
                    (bool flag, string ErrMsg) info = Check_Punish_Incorrect(Max_Number_Trials, Max_Schedule_Time, End_Summary_Condition, End_Summary_Trials_Completed);

                    if (info.flag)
                    { }


                    else
                    {
                        // type a message 
                        ErrorMessage1 += info.ErrMsg;
                    }
                }

                //*************QC Rules Specific to PAL and the following Sub_Analysis: 1: PAl Acquisition  2: PAl Analysis********

                if (
                    ((Analysis_Name.Trim().ToLower().Contains("pal analysis") || string.IsNullOrEmpty(Analysis_Name)) && (Schedule_Name.Trim().ToLower().Contains("pal")) &&

                    (Schedule_Name.Trim().ToLower().Contains("mouse dpal") || Schedule_Name.Trim().ToLower().Contains("mouse spal")
                    || Schedule_Name.Trim().ToLower().Contains("mouse dpal retention") || Schedule_Name.Trim().ToLower().Contains("mouse spal retention")
                    || Schedule_Name.Trim().ToLower().Contains("dpal simple retention") || Schedule_Name.Trim().ToLower().Contains("dpal simple retention")))


                    || ((Analysis_Name.Trim().ToLower().Contains("pal aquisition") || Analysis_Name.Trim().ToLower().Contains("pal acquisition") || string.IsNullOrEmpty(Analysis_Name)) && Schedule_Name.Trim().ToLower().Contains("pal") && (Schedule_Name.Trim().ToLower().Contains("acquisition") || Schedule_Name.Trim().ToLower().Contains("aquisition")))
                    )

                {
                    if (Schedule_Name.Trim().ToLower().Contains("spal") || Schedule_Name.Trim().ToLower().Contains("dpal"))
                    {
                        if (!(!string.IsNullOrEmpty(Analysis_Name) && Analysis_Name.Trim().ToLower().Contains("pal") && (Schedule_Name.Trim().ToLower().Contains("spal") || Schedule_Name.Trim().ToLower().Contains("dpal"))))
                        {
                            ErrorMessage1 += "Analysis Name does not match with Schedule Name. ";
                        }
                    }
                    else if (Schedule_Name.Trim().ToLower().Contains("aquisition") || Schedule_Name.Trim().ToLower().Contains("acquisition"))
                    {
                        if (!(!string.IsNullOrEmpty(Analysis_Name) && (Analysis_Name.Trim().ToLower().Contains("aquisition") || Analysis_Name.Trim().ToLower().Contains("acquisition")) && (Schedule_Name.Trim().ToLower().Contains("acquisition") || Schedule_Name.Trim().ToLower().Contains("aquisition"))))
                        {
                            ErrorMessage1 += "Analysis Name does not match with Schedule Name. ";
                        }
                    }


                    (bool flag, string ErrMsg) info = Check_PAL_Analysis_Acquisition(Max_Number_Trials, Max_Schedule_Time, End_Summary_Condition, End_Summary_Trials_Completed);


                    if (info.flag)
                    { }

                    else
                    {
                        // type a message 
                        ErrorMessage1 += info.ErrMsg;
                    }
                }

                //*************QC Rules Specific to PD and the following Sub_Analysis: 1: Acquisition  2: Baseline 3: REversal 4: RetentionReversal 5: Maintenance********

                if ((Analysis_Name.Trim().ToLower().Contains("pd analysis") || string.IsNullOrEmpty(Analysis_Name)) && (Schedule_Name.Trim().ToLower().Contains("pd")) &&
                    (Schedule_Name.Trim().ToLower().Contains("acquisition") || Schedule_Name.Trim().ToLower().Contains("aquisition")
                    || Schedule_Name.Trim().ToLower().Contains("baseline") || Schedule_Name.Trim().ToLower().Contains("maintenance")
                    || Schedule_Name.Trim().ToLower().Contains("reversal") || Schedule_Name.Trim().ToLower().Contains("retentionreversal")))
                {

                    if (!(!string.IsNullOrEmpty(Analysis_Name) && Analysis_Name.Trim().ToLower().Contains("pd") && (Schedule_Name.Trim().ToLower().Contains("acquisition") || Schedule_Name.Trim().ToLower().Contains("aquisition") || Schedule_Name.Trim().ToLower().Contains("baseline")
                        || Schedule_Name.Trim().ToLower().Contains("maintenance") || Schedule_Name.Trim().ToLower().Contains("reversal") || Schedule_Name.Trim().ToLower().Contains("retentionreversal"))))
                    {
                        ErrorMessage1 += "Analysis Name does not match with Schedule Name. ";
                    }

                    (bool flag, string ErrMsg) info = Check_PD(Max_Number_Trials, Max_Schedule_Time, End_Summary_Condition, End_Summary_Trials_Completed);

                    if (info.flag)
                    { }

                    else
                    {
                        // type a message  
                        ErrorMessage1 += info.ErrMsg;
                    }
                }

                //***********QC Rules Specific to Cam 5C and Sub_Analysis "Cam Mouse5C Touch Var1 *********************"

                if ((Analysis_Name.Trim().ToLower().Contains("Cam Mouse5C Touch Var1") || Analysis_Name.Contains("Mouse5C Touch Var1") || string.IsNullOrEmpty(Analysis_Name)) && (Schedule_Name.Trim().ToLower().Contains("5c")) &&
                    (Schedule_Name.Trim().ToLower().Contains("8s") || Schedule_Name.Trim().ToLower().Contains("8000ms") || Schedule_Name.Trim().ToLower().Contains("0.8s") || Schedule_Name.Trim().ToLower().Contains("800ms") ||
                    Schedule_Name.Trim().ToLower().Contains("4s") || Schedule_Name.Trim().ToLower().Contains("4000ms") || Schedule_Name.Trim().ToLower().Contains("0.4s") || Schedule_Name.Trim().ToLower().Contains("400ms") ||
                    Schedule_Name.Trim().ToLower().Contains("6s") || Schedule_Name.Trim().ToLower().Contains("6000ms") || Schedule_Name.Trim().ToLower().Contains("0.6s") || Schedule_Name.Trim().ToLower().Contains("600ms") ||
                    Schedule_Name.Trim().ToLower().Contains("2s") || Schedule_Name.Trim().ToLower().Contains("2000ms") || Schedule_Name.Trim().ToLower().Contains("0.2s") || Schedule_Name.Trim().ToLower().Contains("200ms") ||
                    Schedule_Name.Trim().ToLower().Contains("1.5s") || Schedule_Name.Trim().ToLower().Contains("1500ms") ||
                    Schedule_Name.Trim().ToLower().Contains("1s") || Schedule_Name.Trim().ToLower().Contains("1000ms"))


                    )
                {

                    if (!(!string.IsNullOrEmpty(Analysis_Name) && Analysis_Name.Trim().ToLower().Contains("var") && Schedule_Name.Trim().ToLower().Contains("var")))
                    {
                        ErrorMessage1 += "Analysis Name does not match with Schedule Name. ";
                    }

                    (bool flag, string ErrMsg) info = Check_Mouse5C_Touch_Var1(Max_Number_Trials, Max_Schedule_Time, Threshold_Condition, Threshold_Trials);

                    if (info.flag)
                    { }

                    else
                    {
                        // type a message 
                        ErrorMessage1 += info.ErrMsg;
                    }
                }

                if (!string.IsNullOrEmpty(ErrorMessage1))
                {
                    ErrmasgHeading += $@"Error message for file ( {filename} ) is: {Environment.NewLine} {ErrorMessage1} {Environment.NewLine} *********************************************************************************************** {Environment.NewLine}";  
                }

            

            }

            File.WriteAllText(outputFilePath, ErrmasgHeading);
            Application.Exit();
            return;
            
        }

        // Function Definition for extracting the required fields from XML Files ******************
        private string FeatureExtraction(string Tag1, string Tag2, string TagName, string TagValue, XDocument xdoc1)
        {

            string output = "";
            string xpath = "/LiEvent/" + Tag1 + "/" + Tag2 + "[Name='" + TagName + "']/" + TagValue;

            var value = xdoc1.XPathSelectElement(xpath);
            if (value != null)
            {
                output = (string)value;
            }

            return output;

        }

        // Function definition for handleing 1 seconds delay to stop executing
        private bool IsNumberAcceptable(int exactNumber, string Machine_Var)
        {
            bool flag = false;
            if ((float.Parse(HandleNullStr(Machine_Var)) > exactNumber - 1 && float.Parse(HandleNullStr(Machine_Var)) < exactNumber + 1))
            {
                flag = true;
            }


            return flag;
        }

        // Function DEfinition for checking conditions of Habituation 1 for 5C, PAL, and PD
        private (bool flag, string ErrMsg) Check_Habit_1(string Max_Number_Trials, string Max_Schedule_Time, string End_Summary_Condition)
        {
            bool Flag = false;
            string ErrMsg1 = "";
            if ((Int32.Parse(HandleNullStr(Max_Number_Trials)) == 0 || Max_Number_Trials == ""))

            {
                Flag = true;
            }
            else
            {
                if ((Int32.Parse(HandleNullStr(Max_Number_Trials)) != 0 || Max_Number_Trials != ""))
                { ErrMsg1 += $"Max_Number_Trials should be empty, but this value is equal to <b> { Int32.Parse(HandleNullStr(Max_Number_Trials))}  </b> in the uploaded file {Environment.NewLine}"; }


            }

            return (flag: Flag, ErrMsg: ErrMsg1);
        }

        // Function DEfinition for checking conditions of Habituation 2 for 5C, PAL, and PD
        private (bool flag, string ErrMsg) Check_Habit_2(string Max_Number_Trials, string Max_Schedule_Time, string End_Summary_Condition)
        {
            bool Flag = false;
            string ErrMsg1 = "";
            if ((Int32.Parse(HandleNullStr(Max_Number_Trials)) == 0 || Max_Number_Trials == ""))

            {
                Flag = true;
            }
            else
            {
                if ((Int32.Parse(HandleNullStr(Max_Number_Trials)) != 0 || Max_Number_Trials != ""))
                { ErrMsg1 += $"Max_Number_Trials should be empty or 0, but this value is equal to <b>  { Int32.Parse(HandleNullStr(Max_Number_Trials))}  </b> in the uploaded file. {Environment.NewLine}"; }


            }


            return (flag: Flag, ErrMsg: ErrMsg1);
        }

        // Function DEfinition for checking conditions of Initial Touch for 5C, PAL, and PD
        private (bool flag, string ErrMsg) Check_Initial_Touch(string Max_Number_Trials, string Max_Schedule_Time, string End_Summary_Condition, string End_Summary_No_Images, string End_Summary_Trials_Completed)
        {
            bool Flag = false;
            string ErrMsg1 = "";
            if (
                (float.Parse(HandleNullStr(End_Summary_Condition)) > 0)
                && (float.Parse(HandleNullStr(End_Summary_No_Images)) > 0 || float.Parse(HandleNullStr(End_Summary_Trials_Completed)) > 0)
                && (((float.Parse(HandleNullStr(End_Summary_No_Images)) < Int32.Parse(HandleNullStr(Max_Number_Trials)) || float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_No_Images)) == Int32.Parse(HandleNullStr(Max_Number_Trials)) || float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials)))))
                )
            {
                Flag = true;
            }
            else
            {


                if (float.Parse(HandleNullStr(End_Summary_Condition)) <= 0)
                { ErrMsg1 += $@"End_Summary_Condition should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Condition))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (float.Parse(HandleNullStr(End_Summary_No_Images)) <= 0)
                { ErrMsg1 += $@"End_Summary_No_Images should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_No_Images))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (!(((float.Parse(HandleNullStr(End_Summary_No_Images)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_No_Images)) == Int32.Parse(HandleNullStr(Max_Number_Trials))))))
                {
                    ErrMsg1 += $@"End_Summary_Condition has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b> if the End_Summary_No_Images is less than <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> OR
                  End_Summary_No_Images has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> if the End_Summary_Condition is less than <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b>,
                  but such values are set as follows in the uploaded file: {Environment.NewLine}
                  <ul><li>End_Summary_Condition = <b>{float.Parse(HandleNullStr(End_Summary_Condition))}</b></li >
                  <li> End_Summary_No_Images = <b>{float.Parse(HandleNullStr(End_Summary_No_Images))}</b></li ></ul>{Environment.NewLine}";
                }



            }


            return (flag: Flag, ErrMsg: ErrMsg1);
        }

        // Function DEfinition for checking conditions of Must Touch for 5C, PAL, and PD
        private (bool flag, string ErrMsg) Check_Must_Touch(string Max_Number_Trials, string Max_Schedule_Time, string End_Summary_Condition, string End_Summary_Corrects, string End_Summary_Trials_Completed)
        {
            bool Flag = false;
            string ErrMsg1 = "";
            if (
                (float.Parse(HandleNullStr(End_Summary_Condition)) > 0)
                && (float.Parse(HandleNullStr(End_Summary_Corrects)) > 0 || float.Parse(HandleNullStr(End_Summary_Trials_Completed)) > 0)
                && (((float.Parse(HandleNullStr(End_Summary_Corrects)) < Int32.Parse(HandleNullStr(Max_Number_Trials)) || float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Corrects)) == Int32.Parse(HandleNullStr(Max_Number_Trials)) || float.Parse(HandleNullStr(End_Summary_Corrects)) == Int32.Parse(HandleNullStr(Max_Number_Trials)))))
                )
            {
                Flag = true;
            }
            else
            {

                if (float.Parse(HandleNullStr(End_Summary_Condition)) <= 0)
                { ErrMsg1 += $@"End_Summary_Condition should be greater than 0, but this value is equal to  <b>  {float.Parse(HandleNullStr(End_Summary_Condition)) }  </b>   in the uploaded file. {Environment.NewLine}"; }

                if (float.Parse(HandleNullStr(End_Summary_Corrects)) <= 0)
                { ErrMsg1 += $@"End_Summary_Corrects should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Corrects))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (!(((float.Parse(HandleNullStr(End_Summary_Corrects)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Corrects)) == Int32.Parse(HandleNullStr(Max_Number_Trials))))))
                {
                    ErrMsg1 += $@"End_Summary_Condition has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b> if the End_Summary_Corrects is less than <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> OR
                  End_Summary_Corrects has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> if the End_Summary_Condition is less than <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b>,
                  but such values are set as follows in the uploaded file: {Environment.NewLine}
                  <ul><li>End_Summary_Condition = <b>{float.Parse(HandleNullStr(End_Summary_Condition))}</b></li >
                  <li> End_Summary_Corrects = <b>{float.Parse(HandleNullStr(End_Summary_Corrects))}</b></li ></ul>{Environment.NewLine}";

                }



            }


            return (flag: Flag, ErrMsg: ErrMsg1);
        }

        // Function DEfinition for checking conditions of Must Initiate for 5C, PAL, and PD
        private (bool flag, string ErrMsg) Check_Must_Initiate(string Max_Number_Trials, string Max_Schedule_Time, string End_Summary_Condition, string End_Summary_Corrects, string End_Summary_Trials_Completed)
        {
            bool Flag = false;
            string ErrMsg1 = "";
            if (
                (float.Parse(HandleNullStr(End_Summary_Condition)) > 0)
                && (float.Parse(HandleNullStr(End_Summary_Corrects)) > 0 || float.Parse(HandleNullStr(End_Summary_Trials_Completed)) > 0)
                && (((float.Parse(HandleNullStr(End_Summary_Corrects)) < Int32.Parse(HandleNullStr(Max_Number_Trials)) || float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Corrects)) == Int32.Parse(HandleNullStr(Max_Number_Trials)) || float.Parse(HandleNullStr(End_Summary_Corrects)) == Int32.Parse(HandleNullStr(Max_Number_Trials)))))
                )
            {
                Flag = true;
            }
            else
            {

                if (float.Parse(HandleNullStr(End_Summary_Condition)) <= 0)
                { ErrMsg1 += $@"End_Summary_Condition should be greater than 0, but this value is equal to  <b>  {float.Parse(HandleNullStr(End_Summary_Condition)) }  </b>   in the uploaded file. {Environment.NewLine}"; }

                if (float.Parse(HandleNullStr(End_Summary_Corrects)) <= 0)
                { ErrMsg1 += $@"End_Summary_Corrects should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Corrects))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (!(((float.Parse(HandleNullStr(End_Summary_Corrects)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Corrects)) == Int32.Parse(HandleNullStr(Max_Number_Trials))))))
                {
                    ErrMsg1 += $@"End_Summary_Condition has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b> if the End_Summary_Corrects is less than <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> OR
                  End_Summary_Corrects has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> if the End_Summary_Condition is less than <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b>,
                  but such values are set as follows in the uploaded file: {Environment.NewLine}
                  <ul><li>End_Summary_Condition = <b>{float.Parse(HandleNullStr(End_Summary_Condition))}</b></li >
                  <li> End_Summary_Corrects = <b>{float.Parse(HandleNullStr(End_Summary_Corrects))}</b></li ></ul>{Environment.NewLine}";

                }



            }


            return (flag: Flag, ErrMsg: ErrMsg1);
        }

        // Function Definition for checking conditions of Punish Incorrect for 5C, PAL, and PD
        private (bool flag, string ErrMsg) Check_Punish_Incorrect(string Max_Number_Trials, string Max_Schedule_Time, string End_Summary_Condition, string End_Summary_Trials_Completed)
        {
            bool Flag = false;
            string ErrMsg1 = "";

            if (
                (float.Parse(HandleNullStr(End_Summary_Condition)) > 0)
                && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) > 0)
                && (((float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials)))))
                )
            {
                Flag = true;
            }

            else

            {

                if (float.Parse(HandleNullStr(End_Summary_Condition)) <= 0)
                { ErrMsg1 += $@"End_Summary_Condition should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Condition))} </b> in the uploaded file.{Environment.NewLine}"; }

                if (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) <= 0)
                { ErrMsg1 += $@"End_Summary_Trials_Completed should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Trials_Completed))} </b> in the uploaded file.{Environment.NewLine}"; }

                if (!(((float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) <= Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials))))))
                {
                    ErrMsg1 += $@"End_Summary_Condition has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b> if the End_Summary_Trials_Completed is less than <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> OR
                  End_Summary_Trials_Completed has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> if the End_Summary_Condition is less than <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b>,
                  but such values are set as follows in the uploaded file: {Environment.NewLine}
                  <ul><li>End_Summary_Condition = <b>{End_Summary_Condition}</b></li >
                  <li> End_Summary_Trials_Completed = <b>{End_Summary_Trials_Completed}</b></li ></ul>{Environment.NewLine}";
                }
            }

            return (flag: Flag, ErrMsg: ErrMsg1);

        }

        // Function DEfinition for checking conditions of PAL Analysis & PAl Acquisaition 
        private (bool flag, string ErrMsg) Check_PAL_Analysis_Acquisition(string Max_Number_Trials, string Max_Schedule_Time, string End_Summary_Condition, string End_Summary_Trials_Completed)
        {
            bool Flag = false;
            string ErrMsg1 = "";

            if (
                (float.Parse(HandleNullStr(End_Summary_Condition)) > 0)
                && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) > 0)
                && (((float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials)))))
                )
            {
                Flag = true;
            }

            else

            {

                if (float.Parse(HandleNullStr(End_Summary_Condition)) <= 0)
                { ErrMsg1 += $@"End_Summary_Condition should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Condition))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) <= 0)
                { ErrMsg1 += $@"End_Summary_Trials_Completed should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Trials_Completed))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (!(((float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials))))))
                {
                    ErrMsg1 += $@"End_Summary_Condition has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b> if the End_Summary_Trials_Completed is less than <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> OR
                  End_Summary_Trials_Completed has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> if the End_Summary_Condition is less than <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b>,
                  but such values are set as follows in the uploaded file: {Environment.NewLine}
                  <ul><li>End_Summary_Condition = <b>{End_Summary_Condition}</b></li >
                  <li> End_Summary_Trials_Completed = <b>{End_Summary_Trials_Completed}</b></li ></ul>{Environment.NewLine}";

                }
            }

            return (flag: Flag, ErrMsg: ErrMsg1);

        }

        // Function DEfinition for checking conditions of Sub-Analysis whcih are Specific to PD 
        private (bool flag, string ErrMsg) Check_PD(string Max_Number_Trials, string Max_Schedule_Time, string End_Summary_Condition, string End_Summary_Trials_Completed)
        {
            bool Flag = false;
            string ErrMsg1 = "";
            if (
               (float.Parse(HandleNullStr(End_Summary_Condition)) > 0)
                && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) > 0)
                && (((float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials)))))
                )
            {
                Flag = true;
            }

            else

            {

                if (float.Parse(HandleNullStr(End_Summary_Condition)) <= 0)
                { ErrMsg1 += $@"End_Summary_Condition should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Condition))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) <= 0)
                { ErrMsg1 += $@"End_Summary_Trials_Completed should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Trials_Completed))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (!(((float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials))))))
                {
                    ErrMsg1 += $@"End_Summary_Condition has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b> if the End_Summary_Trials_Completed is less than <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> OR
                  End_Summary_Trials_Completed has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> if the End_Summary_Condition is less than <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b>,
                  but such values are set as follows in the uploaded file: {Environment.NewLine}
                  <ul><li>End_Summary_Condition = <b>{End_Summary_Condition}</b></li >
                  <li> End_Summary_Trials_Completed = <b>{End_Summary_Trials_Completed}</b></li ></ul>{Environment.NewLine}";
                }
            }


            return (flag: Flag, ErrMsg: ErrMsg1);

        }

        // Function Definition for checking conditions of Sub_Analysis which are Specific to 5C or Cam 5C
        private (bool flag, string ErrMsg) Check_Mouse5C_Touch_Var1(string Max_Number_Trials, string Max_Schedule_Time, string Threshold_Condition, string Threshold_Trials)
        {
            bool Flag = false;
            string ErrMsg1 = "";

            if (
                (float.Parse(HandleNullStr(Threshold_Condition)) > 0)
                && (float.Parse(HandleNullStr(Threshold_Trials)) > 0)
                && (((float.Parse(HandleNullStr(Threshold_Trials)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), Threshold_Condition)) ||
                ((float.Parse(HandleNullStr(Threshold_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(Threshold_Trials)) == Int32.Parse(HandleNullStr(Max_Number_Trials)))))
                )
            {
                Flag = true;
            }

            else
            {

                if (float.Parse(HandleNullStr(Threshold_Condition)) <= 0)
                { ErrMsg1 += $@"Threshold_Condition should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(Threshold_Condition))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (float.Parse(HandleNullStr(Threshold_Trials)) <= 0)
                { ErrMsg1 += $@"Threshold_Trials should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(Threshold_Trials))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (!((((float.Parse(HandleNullStr(Threshold_Trials)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), Threshold_Condition)) ||
                ((float.Parse(HandleNullStr(Threshold_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(Threshold_Trials)) == Int32.Parse(HandleNullStr(Max_Number_Trials)))))))
                {
                    ErrMsg1 += $@"Threshold_Condition has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b> if the Threshold_Trials is less than <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> OR
                  Threshold_Trials has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> if the Threshold_Condition is less than <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b>,
                  but such values are set as follows in the uploaded file: {Environment.NewLine}
                  <ul><li>Threshold_Condition = <b>{Threshold_Condition}</b></li >
                  <li> Threshold_Trials = <b>{Threshold_Trials}</b></li ></ul>{Environment.NewLine}";
                }
            }

            return (flag: Flag, ErrMsg: ErrMsg1);
        }

        // Function Definition for checking the condtions of Sub-Analysis specific to "LD 1 choice" and "LD 1 choice reversal"
        private (bool flag, string ErrMsg) Check_LD1_Choice(string Max_Number_Trials, string Max_Schedule_Time, string End_Summary_Trials_Completed, string End_Summary_Condition)
        {
            bool Flag = false;
            string ErrMsg1 = "";

            if (
                (float.Parse(HandleNullStr(End_Summary_Condition)) > 0)
                && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) > 0)
                && (((float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials))))))
            {
                Flag = true;
            }


            if (Flag != true)
            {

                if (float.Parse(HandleNullStr(End_Summary_Condition)) <= 0)
                { ErrMsg1 += $@"End_Summary_Condition should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Condition))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) <= 0)
                { ErrMsg1 += $@"End_Summary_Trials_Completed should be greater than 0, but this value is equal to <b> { float.Parse(HandleNullStr(End_Summary_Trials_Completed))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (!(((float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials))))))
                {
                    ErrMsg1 += $@"End_Summary_Condition has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b> if the End_Summary_Trials_Completed is less than <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> OR
                  End_Summary_Trials_Completed has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> if the End_Summary_Condition is less than <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b>,
                  but such values are set as follows in the uploaded file: {Environment.NewLine}
                  <ul><li>End_Summary_Condition = <b>{End_Summary_Condition}</b></li >
                  <li> End_Summary_Trials_Completed = <b>{End_Summary_Trials_Completed}</b></li ></ul>{Environment.NewLine}";
                }
            }

            return (flag: Flag, ErrMsg: ErrMsg1);

        }
        // Function Definition for checking the condtions of Sub-Analysis specific to "LD Block 2 choice E H V2"
        private (bool flag, string ErrMsg) Check_LD2OR3_Choice(string Max_Number_Trials, string Max_Schedule_Time, string End_Summary_Trials_Completed, string End_Summary_Condition)
        {
            bool Flag = false;
            string ErrMsg1 = "";

            if (
                        (float.Parse(HandleNullStr(End_Summary_Condition)) > 0)
                        && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) > 0)
                        && (((float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                        || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials))))))
            {
                Flag = true;
            }

            else
            {

                if (float.Parse(HandleNullStr(End_Summary_Condition)) <= 0)
                { ErrMsg1 += $@"End_Summary_Condition should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Condition))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) <= 0)
                { ErrMsg1 += $@"End_Summary_Trials_Completed should be greater than 0, but this value is equal to <b> {float.Parse(HandleNullStr(End_Summary_Trials_Completed))} </b> in the uploaded file. {Environment.NewLine}"; }

                if (!(((float.Parse(HandleNullStr(End_Summary_Trials_Completed)) < Int32.Parse(HandleNullStr(Max_Number_Trials))) && (IsNumberAcceptable(Int32.Parse(HandleNullStr(Max_Schedule_Time)), End_Summary_Condition)))
                || ((float.Parse(HandleNullStr(End_Summary_Condition)) < Int32.Parse(HandleNullStr(Max_Schedule_Time))) && (float.Parse(HandleNullStr(End_Summary_Trials_Completed)) == Int32.Parse(HandleNullStr(Max_Number_Trials))))))
                {
                    ErrMsg1 += $@"End_Summary_Condition has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b> if the End_Summary_Trials_Completed is less than <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> OR
                  End_Summary_Trials_Completed has to be equal to <b>{Int32.Parse(HandleNullStr(Max_Number_Trials))}</b> if the End_Summary_Condition is less than <b>{Int32.Parse(HandleNullStr(Max_Schedule_Time))}</b>,
                  but such values are set as follows in the uploaded file: {Environment.NewLine}
                  <ul><li>End_Summary_Condition = <b>{End_Summary_Condition}</b></li >
                  <li> End_Summary_Trials_Completed = <b>{End_Summary_Trials_Completed}</b></li ></ul>{Environment.NewLine}";
                }
            }

            return (flag: Flag, ErrMsg: ErrMsg1);
        }

        public string HandleNullStr(string varName1)
        {
            if (string.IsNullOrEmpty(varName1))
            {
                return "0";
            }
            return varName1;
        }

        
    }
}
