#!/opt/anaconda/bin/python
'''
Created on Oct 2, 2015

@author: Shuai Liang

Quality control procedures on xml file for ABeD projects.

Need the following files/scripts to run:
0. fileQC.conf
1. xmlConvert.py
2. my_utils.py
3. send_mail.py
4. Files noted in 0. fileQC.conf, e.g., qc.xml, qc_condition.xml,  LONPD_ID_list.xlsx
'''

import sys
import logging
import datetime
import glob
import re
import time
import os
from lxml import etree
import xlrd
from collections import OrderedDict

from xmlConvert import xmlConvert  # Chen Jin's script with minimal change
from my_utils import findtextExt
from my_utils import xpathExt
from send_mail import send_mail
     

def get_IDplusDate(xmlfile, xTree, failedFiles, failedFnames):
    try:
        animalID = (xpathExt(xTree, '//ns:Animal_ID', namespaces=XNAT_NAMESPACE)[0].text).lower()
        expDateTime = xpathExt(xTree, '//ns:Schedule_Start_Time', namespaces=XNAT_NAMESPACE)[0].text
        expDate = (datetime.datetime.strptime(expDateTime, '%Y-%m-%dT%H:%M:%S.%f')).date()
        return animalID + expDate.strftime('%Y%m%d')
        #return IDplusDate
    except Exception as err:
        #  Animal ID has already been checked, only date can be wrong
        failedFiles.append([xmlfile, "Schedule_Start_Time: not found."] ) 
        failedFnames.append(xmlfile)
        try:
            expDateTime = xpathExt(xTree, '//ns:Exp_Date_Time', namespaces=XNAT_NAMESPACE)[0].text
            expDate = (datetime.datetime.strptime(expDateTime, '%m/%d/%Y %H:%M:%S %p')).date()
            return animalID + expDate.strftime('%Y%m%d')
        except:
            return animalID + " unknown"
            

def check_end_conditions(xmlfile, xTree, EndConditions, failedFiles, failedFnames):
    #print xmlfile
    nsMap = {'ns':'http://nrg.wustl.edu/abx'}
    for cond in EndConditions:
        if cond[0] == "others":
            evalMarkerName = xTree.xpath('//ns:Marker//ns:Name[text()=$COND]',
                                         COND=cond[1], namespaces=nsMap)
            if not evalMarkerName:
                failedFiles.append([xmlfile, cond[1]]) 
                failedFnames.append(xmlfile)      
            # later we might want to check the number of Results
            #if len(cond) == 3:            
            #    evalMarkerResults = evalMarkerName[0].getnext()            
            #    print evalMarkerResults.tag, evalMarkerResults.text
            return
        else:
            namePattern = re.compile(cond[0])
            if namePattern.match(xmlfile):
                if not cond[1]:
                    return
                evalMarkerName = xTree.xpath('//ns:Marker//ns:Name[text()=$COND]',
                                             COND=cond[1], namespaces=nsMap)
                if not evalMarkerName:
                    failedFiles.append([xmlfile, cond[1]])  
                    failedFnames.append(xmlfile)
                return              

def get_xml_files(dir):
    files = []
    for root, dirname, filenames in os.walk(dir):
        for fname in filenames:
            if fname.endswith('.xml'):
                files.append(os.path.join(root, fname))    
    #if len(files) < 1:
    #    raise ValueError("Find no xml files.")     
    return files

def read_conf_file(fname):    
    try:
        conf = {}
        with open(fname, 'r') as confFile:
            conf_rt = etree.parse(confFile)
        conf['mailSender'] = findtextExt(conf_rt, '//email/sender')
        receivers = findtextExt(conf_rt, "//email/receivers")
        conf['mailReceivers'] = receivers.split(',')
        conf['mailSubject'] = findtextExt(conf_rt, "//email/subject")
        conf['mailBody'] = findtextExt(conf_rt, "//email/body")

        conf['rawFolder'] = findtextExt(conf_rt, '//rawFolder')        
        conf['passedFolder'] = findtextExt(conf_rt, '//passedFolder') 
        conf['junkFolder'] = findtextExt(conf_rt, '//junkFolder')
        conf['ignoredFolder'] = findtextExt(conf_rt, '//ignoredFolder')  
        conf['flaggedFile'] = findtextExt(conf_rt, '//flaggedFile')
        conf['completedFile'] = findtextExt(conf_rt, '//completedFile')
        conf['incompletedFile'] = findtextExt(conf_rt, '//incompletedFile')
        
        conf['valueCheckFile'] = findtextExt(conf_rt, '//valueCheckFile')  
        conf['valueConditionFile'] = findtextExt(conf_rt, '//valueConditionFile')  
        conf['sessionCountFile'] = findtextExt(conf_rt, '//sessionCountFile')
        conf['mapFile'] = findtextExt(conf_rt, '//mapFile')         
        
        try:
            conf['IDlists'] = conf_rt.findtext('//IDlists')            
        except:
            pass
        
        return conf               
    except Exception as err:    
        logging.error(err)
        raise  

def get_schedule_conditions(fieldValues, ABeDDataBase, ABeDSchedule):
    
    for dbase in fieldValues:
        dbNames = dbase.get("name").split('|||')
        for dbName in dbNames:
            if re.compile(dbName).match(ABeDDataBase.text):
                for schedule in dbase:
                    if schedule.tag is etree.Comment: # ignore comments                        
                        continue
                    scheduleName = schedule.find("name").text  
                    
                    if re.compile(scheduleName, re.IGNORECASE).match(ABeDSchedule.text):
                        return schedule   
    # If no schedule is found, then nothing to check, just return None.
    return None                    

def get_marker_name_value(xTree, cond):
    markerName = xTree.xpath('//ns:Marker//ns:Name[text()=$COND]',
                             COND=cond[0].text, 
                             namespaces=XNAT_NAMESPACE
                             )   
    markerValue = None
    if len(markerName) > 0:
        markerValue = (markerName[0].getnext()).text                          
    else:
        markerName = xTree.xpath("//*[local-name() = $COND]", 
                                 COND=cond[0].text,
                                 namespaces=XNAT_NAMESPACE
                                 )   
        if len(markerName) > 0:                    
            markerValue = markerName[0].text    
    
    return markerName, markerValue      

def check_values(xmlFile, xTree, failedFiles, failedFnames):
    try:
        with open(CONF['valueCheckFile'], 'r') as vChecker:
            fieldValues = (etree.parse(vChecker)).getroot()
        ABeDDataBase = xpathExt(xTree, "//ns:Database", namespaces=XNAT_NAMESPACE)[0]
        ABeDSchedule = xpathExt(xTree, "//ns:Schedule_Name", namespaces=XNAT_NAMESPACE)[0]                                      
        scheduleConditions = get_schedule_conditions(fieldValues, ABeDDataBase, ABeDSchedule)
        
        if scheduleConditions is None:
            #print "No condition for file: ", xmlFile, " in check_values."
            return  

        for cond in scheduleConditions[1:]:
            # cond[0] is the field name, cond[1] is the value/condition
            markerName, markerValue = get_marker_name_value(xTree, cond)                        
            
            if cond[1].text == "0|||empty":
                if len(markerName) == 0 or int(markerValue) == 0:
                    pass
                else:
                    failedFiles.append([xmlFile, cond[0].text, "Not found"])
                    failedFnames.append(xmlFile)
            elif len(markerName) == 0 or markerValue is None:
                failedFiles.append([xmlFile, cond[0].text, "Not found"])
                failedFnames.append(xmlFile)             
            else:
                if any(x in cond[1].text for x in "><"):
                    if not eval(markerValue + cond[1].text):
                        failedFiles.append([xmlFile, cond[0].text, markerValue])
                        failedFnames.append(xmlFile)
                elif cond[0].text == "Analysis_Name":
                    if cond[1].text not in markerValue:
                        failedFiles.append([xmlFile, cond[0].text, markerValue])
                        failedFnames.append(xmlFile)
                else:                                    
                    expectedValues = [int(v) for v in (cond[1].text).split("|||")]
                    if int(float(markerValue)) not in expectedValues:
                        failedFiles.append([xmlFile, cond[0].text, markerValue])
                        failedFnames.append(xmlFile)

    except Exception as err:
        raise    

def check_value_conditions(xmlFile, xTree, failedFiles, failedFnames):
    # Check conditional values, 
    # if a < given value, b should equal to the given value
    # this is done after "check_values", the values must be there for comparing
    try:
        with open(CONF['valueConditionFile'], 'r') as vCond:
            fieldConditions = (etree.parse(vCond)).getroot() 
        ABeDDataBase = xpathExt(xTree, "//ns:Database", namespaces=XNAT_NAMESPACE)[0]
        ABeDSchedule = xpathExt(xTree, "//ns:Schedule_Name", namespaces=XNAT_NAMESPACE)[0]                                      
        scheduleConditions = get_schedule_conditions(fieldConditions, ABeDDataBase, ABeDSchedule)
        if scheduleConditions is None:
            logging.warning("No condition for file: " + xmlFile + " in check_value_conditions.")
            return          
        if len(scheduleConditions) !=3:
            raise ValueError("Value condition checking took exactly 2 condition")
        else:
            condValue = int(scheduleConditions[1][1].text)   # given value of the field
            corrValue = int(scheduleConditions[2][1].text)
            markerName1, endCondition = get_marker_name_value(xTree, scheduleConditions[1])
            markerName2, endCorrects = get_marker_name_value(xTree, scheduleConditions[2])
            if (int(endCorrects) < corrValue and int(float(endCondition)) != condValue) or \
                   (int(float(endCondition)) < condValue and int(endCorrects) != corrValue):
                failedFiles.append([xmlFile, markerName1[0].text, endCondition, 
                                        markerName2[0].text, endCorrects])
                failedFnames.append(xmlFile)
    except Exception as err:
        raise    

def write_failures(flaggedFiles, allIDDate):
    failures = list(set([f[0] for f in flaggedFiles]))
    with open(CONF['flaggedFile'], 'w') as flagF:
        flagF.write('# Flagged %d Files for missing/incorrect field values.\n' \
                     %(len(failures)))
        for f in flaggedFiles:
            IDFound = False
            for k, v in allIDDate.iteritems(): # record mouse ID and date to the flagged files 
                if f[0] in v:
                    flagF.write(k[:-8] + ",  " + k[-8:] + ",  " + ', '.join(f)+'\n')
                    IDFound = True
            if not IDFound:
                flagF.write(', '.join(f)+'\n')                
        
        repeatedIDs = {}
        for k, v in allIDDate.iteritems():
            repeatedIDs[k] =  [x for x in v if x not in failures]                       
        flagF.write('# %d Mouse IDs are found to appear in the same day.\n' \
                     %len([x for x in repeatedIDs.values() if len(x) > 1]))        
        for k, v in repeatedIDs.iteritems():
            if len(v) > 1:
                flagF.write(k[:-8] + ",  " + k[-8:] + ",  " + ', '.join(v)+'\n')                      

def get_ID_list():
    # A bit misleading. But the function returns a dictionary, with the task (or others specified 
    # in the 1st column) as the key
    xl_workbook = xlrd.open_workbook(CONF['IDlists'])
    sheet_names = xl_workbook.sheet_names()
    xl_sheet = xl_workbook.sheet_by_name(sheet_names[0])
    subIDs = []
    sheetDict = {}
    for colnum in range(0, xl_sheet.ncols):        
        for rownum in range(1,xl_sheet.nrows):
            id = xl_sheet.cell(rownum, colnum).value
            if id:
                sheetDict.setdefault(xl_sheet.cell(0,colnum).value, []).append(id.lower()) # allow mixing use of lower and upper cases
    return sheetDict                
    
def check_animalIDs(xmlFile, xTree, failedFiles, failedFnames):
    idList = get_ID_list()
    animalID = xpathExt(xTree, '//ns:Animal_ID', namespaces=XNAT_NAMESPACE)[0].text #).lower() # allow mixing use of lower and upper cases
    for k in idList:
        task = re.compile(".*"+k, re.IGNORECASE)
        if task.match(xmlFile):
            if animalID.lower() not in idList[k]:            
                failedFiles.append([xmlFile, "Animal ID: " + animalID + " not in the given list."] )
                failedFnames.append(xmlFile)

def check_session_numbers(xmlFile, xTree, allIDSchedule):    
    with open(CONF['sessionCountFile'], 'r') as vCond:
        sessionCounts = (etree.parse(vCond, etree.XMLParser(remove_comments=True))).getroot()

    ABeDDataBase = xpathExt(xTree, "//ns:Database", namespaces=XNAT_NAMESPACE)[0].text
    ABeDDataBase = ABeDDataBase.split("_")[0]
    ABeDSchedule = xpathExt(xTree, "//ns:Schedule_Name", namespaces=XNAT_NAMESPACE)[0].text   
    
    for dbase in sessionCounts:
        dbNames = dbase.get("name").split('|||')
        for dbName in dbNames:
            if re.compile(dbName, re.IGNORECASE).match(ABeDDataBase):             
                for schedule in dbase:
                    if schedule.tag is etree.Comment: # ignore comments                        
                        continue
                    scheduleName = schedule.find("name").text                   
                    if re.compile(scheduleName, re.IGNORECASE).match(ABeDSchedule):
                        keyword = schedule.find("keyword").text
                        sessionNumber = schedule.find("sessionNumber").text
                        animalID = (xpathExt(xTree, '//ns:Animal_ID', namespaces=XNAT_NAMESPACE)[0].text).lower()
                        expDateTime = xpathExt(xTree, '//ns:Schedule_Start_Time', namespaces=XNAT_NAMESPACE)[0].text
                        sessionKey = "%s--%s--%s--%s" %(ABeDDataBase, animalID, keyword, sessionNumber)
                        allIDSchedule.setdefault(sessionKey, []).append([xmlFile, expDateTime]) 
 
def write_complete(allIDSchedule):
    # parse sessionNumber as set in function "check_session_numbers". 
    # sessionKey = "%s--%s--%s--%s" %(ABeDDataBase, animalID, keyword, sessionNumber)
    # so sessionNumber = k.split("--")[-1]
    orderAll = OrderedDict(sorted(allIDSchedule.items(), key=lambda x: (x[0].split("--")[0], 
                                                                        x[0].split("--")[1], 
                                                                        x[0].split("--")[2]
                                                                        )))
    with open(CONF['completedFile'], 'w') as cf:
        cf.write("Compeleted list: \n")
    with open(CONF['incompletedFile'], 'w') as icf:
        icf.write("Incompeleted list: \n")        
    for k, v in orderAll.items():
        sessionNumber = k.split("--")[-1]
        actualSessions = len(v)
        v.sort(key = lambda x: x[-1])
        if eval(str(actualSessions) + sessionNumber):
            with open(CONF['completedFile'], 'a') as cf:
                cf.write(k + "\n")
                for f in v:
                    cf.write("   ".join(f) + "\n")
                cf.write("-"*100 + "\n")
        else:
            with open(CONF['incompletedFile'], 'a') as icf:
                icf.write("%s  (expect %s vs. actual %d) \n" %("--".join(k.split("--")[0:-1]), 
                                                               k.split("--")[-1], actualSessions))
                for f in v:
                    icf.write("   ".join(f) + "\n")
                icf.write("-"*100 + "\n")
   
def flag_errors(XMLFiles):    
    # keep two list of failed files. file name + reasons for failures, and file names only.
    failedFiles = []  # file name + reasons for failures
    failedFnames = [] # file names only
    
    allIDDate = {} # for record the date and ID of each file, to be used by any function that needs this info
    allIDSchedule = {} # for counting the sessions each mouse completed, only good sessions are counted.
    for xmlFile in XMLFiles:
    #for xmlFile in ["/home/mousecage/dumpdata/XML files PAL/LONPALAPP4MM_cabA/MOUSEBOX-A_LONPALAPP4MM_Mouse dPAL 1 v3_246.xml"]:
        try:
            xnatRoot = xmlConvert(CONF['mapFile'], xmlFile)
        except Exception as err:
            failedFiles.append([xmlFile, str(err)] )
            continue            
        try:
            rawTree = etree.fromstring(xnatRoot)
            rootTree = rawTree.getroottree() 
            xTree = rootTree.getroot()
            if 'IDlists' in CONF:
                check_animalIDs(xmlFile, xTree, failedFiles, failedFnames)
            if xmlFile not in failedFnames:         
                check_values(xmlFile, xTree, failedFiles, failedFnames)   
            if xmlFile not in failedFnames:
                check_value_conditions(xmlFile, xTree, failedFiles, failedFnames) 
            #if xmlFile not in failedFnames:   
            IDnDate = get_IDplusDate(xmlFile, xTree, failedFiles, failedFnames)
            if IDnDate: 
                allIDDate.setdefault(IDnDate, []).append(xmlFile)    
            if xmlFile not in failedFnames:  
                check_session_numbers(xmlFile, xTree, allIDSchedule)
        except Exception as err:
            logging.error(sys.exc_info())
            continue
    flaggedFiles = failedFiles
    write_complete(allIDSchedule)                
    write_failures(flaggedFiles, allIDDate)   
    # only return the number of errors found                      
    return len(flaggedFiles) + len([x for x in allIDDate.values() if len(x) > 1])
  
                
def main():
    passedFiles = []
    
    allXMLFiles = get_xml_files(CONF['rawFolder'])
    junkFiles = get_xml_files(CONF['junkFolder'])     
    junks = [f.split('/')[-1] for f in junkFiles]
    
    ignoredFiles =  get_xml_files(CONF['ignoredFolder'])
    ignored =  [f.split('/')[-1] for f in ignoredFiles]
    
    XMLFiles = [f for f in allXMLFiles if f.split('/')[-1] not in junks and f.split('/')[-1] not in ignored]
    #XMLFiles = ["/backup/abeddata/GuelphData/GUE5C/GUE5C5FAD9MM/CI23002-5_GUE5C3XTG9MM_5CSRTT_600ms_var1_244.xml"]
    
    print "Total number of xml files: ", len(allXMLFiles)
    print "Need to check ", len(XMLFiles), " files."
    
    NFlags = flag_errors(XMLFiles)

    send_mail(CONF['mailSender'], CONF['mailReceivers'], CONF['mailSubject'], 
          CONF['mailBody'], [CONF['flaggedFile'], CONF['incompletedFile'], CONF['completedFile']]) 

   
if __name__ == '__main__':
    START_TIME = time.time()
    
    CONF_FILE = os.path.dirname(os.path.abspath(sys.argv[0])) + "/fileQC.conf"
    LOG_FILE = os.path.dirname(os.path.abspath(sys.argv[0])) + "/fileQC.log_" + \
                  (datetime.datetime.now()).strftime('%Y%m%d')                            
    logging.basicConfig(filename=LOG_FILE, level=logging.DEBUG)    
    CONF = read_conf_file(CONF_FILE)    
    XNAT_NAMESPACE = {'ns':'http://nrg.wustl.edu/abx'}    
    main()    
    
    print "-"*50
    print " %s seconds. " % (time.time() - START_TIME)
    
