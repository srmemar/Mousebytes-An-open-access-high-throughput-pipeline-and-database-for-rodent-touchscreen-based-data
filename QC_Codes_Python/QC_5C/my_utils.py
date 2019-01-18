'''
Created on Aug 27, 2015

@author: Shuai Liang

Extended/customed Element classes in lxml
'''

from lxml import etree


def findtextExt(elem, xpath, namespaces=None, message=None):
    txt = elem.findtext(xpath, namespaces=namespaces)
    if not txt:
        if not message:
            message = 'Error: Cannot find field %s.' %xpath        
        raise ValueError(message)
    return txt

def xpathExt(elem, xpath, namespaces=None, extensions=None, message=None):
    # 
    txt = elem.xpath(xpath, namespaces=namespaces)
    if not txt:
        if not message:
            message = 'Error: Cannot find field %s.' %xpath
        raise ValueError(message)
    return txt
    