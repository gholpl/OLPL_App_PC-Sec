using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OLPL_App_PC_Sec
{
    class functions
    {
        public bool regCreateKeys(settings st1)
        {
            try
            {
                if (Registry.LocalMachine.OpenSubKey(st1.baseKey) == null)
                {
                    Registry.LocalMachine.CreateSubKey(st1.baseKey);
                }
                if (Registry.LocalMachine.OpenSubKey(st1.baseKey + "\\" + st1.appKey) == null)
                {
                    Registry.LocalMachine.CreateSubKey(st1.baseKey + "\\" + st1.appKey);
                }
            }
            catch(Exception e)
            {
                writeToFile(e.Message, st1);
            }
            return true;
        }
        public settings getSettings(settings st1)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(st1.baseKey + "\\" + st1.appKey, false);
                st1.adminName = (byte[])rk.GetValue("u1");
                st1.adminPass = (byte[])rk.GetValue("p1");
                st1.connectPass = (byte[])rk.GetValue("p2");
                st1.connectUser = (byte[])rk.GetValue("u2");
                st1.adminPass1 = (byte[])rk.GetValue("p1-1");
                st1.timeChanged= (string)rk.GetValue("pC");
            }
            catch (Exception e)
            {
                writeToFile(e.Message, st1);
            }
            
            return st1;
        }
        public string checkAdministrator(settings st1,string userName)
        {
            try
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Machine);

                // find a user
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userName);
                if ((bool)user.Enabled)
                {
                    user.Enabled = false;
                    user.Save();
                    DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                    DirectoryEntry HostedUser = AD.Children.Find(userName, "user");
                    HostedUser.Invoke("SetPassword", new object[] { decryptByte(st1.adminPass) });
                    HostedUser.Close();
                    AD.Close();
                    return "Warning -- " + userName + " is Enabled -- Fixed";
                }
                else
                {
                    DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                    DirectoryEntry HostedUser = AD.Children.Find(userName, "user");
                    HostedUser.Invoke("SetPassword", new object[] { decryptByte(st1.adminPass) });
                    HostedUser.Close();
                    AD.Close();
                    return "OK";
                }
                
            }
            catch (Exception e)  
            { 
                writeToFile(e.Message, st1);
                return "Error";
            }
            
        }
        public string checkAdministrators(settings st1)
        {
            string str1 = "";
            try
            {
                
                DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName);
                DirectoryEntry admGroup = localMachine.Children.Find("administrators", "group");
                object members = admGroup.Invoke("members", null);
                foreach (object groupMember in (IEnumerable)members)
                {
                    DirectoryEntry member = new DirectoryEntry(groupMember);
                    str1 = str1 + " " + member.Name + " ; ";
                }
            }
            catch (Exception e)
            {
                writeToFile(e.Message, st1);
                return "Error";
            }
            return str1;
        }
        public bool sendResults(settings st1,results rs1)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    if (rs1.Result_Admin_Group == null) { rs1.Result_Admin_Group = "Not Reported"; }
                    if (rs1.Result_Admin_User == null) { rs1.Result_Admin_User = "Not Reported"; }
                    if(rs1.Resutl_User_Pass_Changed == null){ rs1.Resutl_User_Pass_Changed = st1.timeChanged; }
                    NameValueCollection vals = new NameValueCollection();
                    vals.Add("CPUName", Environment.MachineName);
                    vals.Add("Result_Admin_Group", rs1.Result_Admin_Group);
                    vals.Add("Result_Admin_User", rs1.Result_Admin_User);
                    vals.Add("Result_Maint_User", rs1.Result_Maint_User);
                    vals.Add("Result_User_Pass_Changed", rs1.Resutl_User_Pass_Changed);
                    client.Credentials = new NetworkCredential(decryptByte(st1.connectUser), decryptByte(st1.connectPass), "olpl");
                    client.UploadValues(st1.resultURL, vals);
                }
            }
            catch (Exception e)
            {
                writeToFile(e.Message, st1);
                return false;
            }
            
            return true;
        }
        public string decryptByte(byte[] bt1)
        {
            return UnicodeEncoding.Unicode.GetString(Encryption.AESGCM.SimpleDecryptWithPassword(bt1, "|k?(){aIbu9~d1T"));
        }
        public byte[] encryptString(string st1)
        {
            return Encryption.AESGCM.SimpleEncryptWithPassword(UnicodeEncoding.Unicode.GetBytes(st1), "|k?(){aIbu9~d1T");
        }
        public bool writeToFile(string str1,settings st1)
        {
            File.WriteAllText(st1.logFile, str1);
            return true;
        }
        public results checkMaintUser(settings st1, results rs1)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Machine);
            string str1 = "";
            bool changePass = true;
            // find a user
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, decryptByte(st1.adminName));
            if (user == null)
            {
                changePass = false;
                PrincipalContext systemContext = null;
                systemContext = new PrincipalContext(ContextType.Machine, null);
                UserPrincipal userPrincipal = new UserPrincipal(systemContext);
                userPrincipal.Name = decryptByte(st1.adminName);
                userPrincipal.DisplayName = "IT Administrative User";
                userPrincipal.PasswordNeverExpires = true;
                userPrincipal.SetPassword(decryptByte(st1.adminPass));
                userPrincipal.Enabled = true;
                userPrincipal.Save();
                GroupPrincipal groupPrincipal = null;
                groupPrincipal = GroupPrincipal.FindByIdentity(systemContext, "Administrators");
                groupPrincipal.Members.Add(userPrincipal);
                groupPrincipal.Save();
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(st1.baseKey + "\\" + st1.appKey, true);
                rk.SetValue("p1-1", st1.adminPass, RegistryValueKind.Binary);
                rk.SetValue("pC", DateTime.Now.ToString(),RegistryValueKind.String);
                rs1.Resutl_User_Pass_Changed = DateTime.Now.ToString();

                str1 =str1+ " not found Created";
            }
            else
            {
                str1 = str1 + " Found";
                PrincipalContext systemContext = null;
                systemContext = new PrincipalContext(ContextType.Machine, null);
                GroupPrincipal groupPrincipal = null;
                groupPrincipal = GroupPrincipal.FindByIdentity(systemContext, "Administrators");
                if (groupPrincipal.Members.Contains(systemContext, IdentityType.SamAccountName, decryptByte(st1.adminName)))
                {
                    str1 = str1 + " Administrator";
                }
                else
                {
                    UserPrincipal usr = UserPrincipal.FindByIdentity(systemContext, decryptByte(st1.adminName));
                    groupPrincipal.Members.Add(usr);
                    groupPrincipal.Save();

                    str1 = str1 + " not Administrator";
                }
            }
            if (ByteArrayCompare(st1.adminPass,st1.adminPass1)!=true && changePass==true)
            {
                try
                {

                    str1 = str1 + " Password does not match";
                    PrincipalContext systemContext = null;
                    systemContext = new PrincipalContext(ContextType.Machine, null);
                    UserPrincipal usr = UserPrincipal.FindByIdentity(systemContext, decryptByte(st1.adminName));
                    usr.ChangePassword(decryptByte(st1.adminPass1), decryptByte(st1.adminPass));
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(st1.baseKey + "\\" + st1.appKey, true);
                    rk.SetValue("p1-1", st1.adminPass, RegistryValueKind.Binary);
                    rk.SetValue("pC", DateTime.Now.ToString(), RegistryValueKind.String);
                    rs1.Resutl_User_Pass_Changed = DateTime.Now.ToString();
                }
                catch(Exception e)
                {
                    str1 = e.Message;
                }
                
            }
            else { str1 = str1 + " Password OK"; }
            rs1.Result_Maint_User = str1;
            return rs1;
        }
        public bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(a1, a2);
        }
    }
}
