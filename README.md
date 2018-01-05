# MadSpy

Category:
Malware Design 
 
Motivation:
As a top secret agent for NSA fighting to save the world, your mission is to gather 
intelligence on a terrorist network that uses a secret chat program for plotting terrorist attacks. 
We have already identified one of the group members. Your mission is to secretly record the 
password when the terrorist is logging in and also take screenshots from the terrorists computer 
to figure out other members of the group.  
 
Description:
You have to use your skills from the MAD course and design a malware that will 
be implanted onto the terrorist’s computer and so you can monitor the target right here from HQ. 
Create a spyware ( a malware with the goal to spy ) that when infects a target computer, 
persists by self starting every time the machine is booted on,  is able to identify when the 
specific chat program is started, record the keystrokes to get the password, take screenshots 
and email the collected information to yourself. 

Project Implementation:
Assume the target computer has Windows 10 installed and your 
program (spyware) is running with administrator privileges (if required). You have to create a 
program to demonstrate three key behaviors of a spyware: 
1. Persistence: The program adds itself to system’s startup list to ensure it is executed on 
system startup. Several methods exists for this. Easiest is Registry modification. 
2. Spy modules: 
a. Enumerating running programs: You need to look into all running processes and 
identify is the target program (assume “skype.exe”) is running or not. If not, you 
wait until it is loaded into memory. 
b. Once the target program has been loaded into memory, you start recording the 
keystrokes (on a text file). Also, you take 5 snapshots of the screen, one every 30 
seconds.  
3.  Exfiltration: Now that you have all you need, you send the data as an email to yourself. 
Note: Design the program in a modular fashion. Keep the target program name, number of 
screenshots and time between screenshots part of a separate config file. This will help testing. 

Deliverables:​
 The spyware executable and all source code. 
 
References: 
1. You may borrow code from: 
http://www.codeproject.com/Articles/2082/API­hooking­revealed 
2. Enumerating processes and Image load notification:  
a. https://msdn.microsoft.com/en­us/library/windows/desktop/ms682623(v=vs.85).aspx 
b. https://msdn.microsoft.com/en­us/library/windows/hardware/ff559957(v=vs.85).aspx 
3. Registry Keys for startup: Note you can access registry keys programmatically using 
system api calls. 
http://www.tenforums.com/tutorials/2944-startup-items-add-delete-enable-disable-windows-10-a.html#option3 
