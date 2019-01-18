'''
Created on Jul 2, 2015

@author: Shuai Liang

Copied from Fan's script (somewhere)
'''

import smtplib

class MailerExtra:
    
    def __init__(self, sender, recipients, subject, header_from, message, mode=0, cc=[], logger=None):
        self.sender = sender
        self.recipients = recipients
        self.subject = subject
        self.mode = mode
        self.cc = cc
        self.message = message
        self.logger = logger
        self.mail_body = self.build_message(header_from)
        self.send()

    def build_message(self, header_from):
        return 'From: {0} \n' \
               'To: {1}\n' \
               'CC: {2}\n'\
               'Subject: {3}\n{4}'\
            .format(header_from, ','.join(self.recipients), ','.join(self.cc), self.subject, self.message)

    def send(self):
        if self.mode == 0:
            self.recipients = ['sliang@research.baycrest.org']

        try:
            smtp = smtplib.SMTP('localhost')
            smtp.sendmail(self.sender, self.recipients + self.cc, self.mail_body)
        except smtplib.SMTPException as e:
            if self.logger:
                self.logger.critical(e)
                
# usage
'''
import MailerExtra
MailerExtra(sender, recipients, subject, header, message, mode, logger)
'''           