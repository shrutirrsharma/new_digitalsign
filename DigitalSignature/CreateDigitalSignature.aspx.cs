using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace DigitalSignature
{
    public partial class CreateDigitalSignature : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        private static Random random = new Random();
        
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        protected void Upload_Files(object sender, EventArgs e)
        {
            if (fileUpload.HasFile)     // CHECK IF ANY FILE HAS BEEN SELECTED.
            {
                int iUploadedCnt = 0;
                int iFailedCnt = 0;
                HttpFileCollection hfc = Request.Files;
                lblFileList.Text = "Select <b>" + hfc.Count + "</b> file(s)";

                if (hfc.Count <= 10)    // 10 FILES RESTRICTION.
                {
                    for (int i = 0; i <= hfc.Count - 1; i++)
                    {
                        HttpPostedFile hpf = hfc[i];
                        if (hpf.ContentLength > 0)
                        {
                            if (!File.Exists(Server.MapPath("CopyFiles\\") +
                                Path.GetFileName(hpf.FileName)))
                            {
                                DirectoryInfo objDir = new DirectoryInfo(Server.MapPath("CopyFiles\\"));

                                
                                string sFileName = Path.GetFileName(hpf.FileName);
                                string sFileExt = Path.GetExtension(hpf.FileName);

                                // CHECK FOR DUPLICATE FILES.
                                FileInfo[] objFI = objDir.GetFiles(sFileName.Replace(sFileExt, "") + ".*");
                                if (objFI.Length > 0)
                                {
                                    foreach (FileInfo file in objFI)
                                    {
                                        string sFileName1 = objFI[0].Name;
                                        string sFileExt1 = Path.GetExtension(objFI[0].Name);

                                        if (sFileName1.Replace(sFileExt1, "") ==  sFileName.Replace(sFileExt, ""))
                                        {
                                            iFailedCnt += 1;        // NOT ALLOWING DUPLICATE.
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    // Generate AccessKey Transaction + AccessKey+TimeStamp

                                    string strTransaction = RandomString(17);
                                    //Production 

                                    string strAccessKey = "57abae53081445e59eba09ca2ccd2cd9";
                                    // UAT 
                                    //string strAccessKey = "485b36776f9b450893e0c17697d4f890";
                                    string strDateFormat = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss+05:30");
                                    string strGeneratesha256_hash = sha256_hash(strTransaction + strAccessKey + strDateFormat);

                                    //Converting pdf into base64.

                                    Byte[] bytesfile = File.ReadAllBytes("D:\\DigitalSignature\\pdfDocument\\" + sFileName);
                                    String file = Convert.ToBase64String(bytesfile);

                                    //Api Integration code.

                                    string strXML = string.Empty;
                                    //Production
                                    string url = "https://remotesigning-prod.emudhra.com/api/Doc/v1/Signature";

                                    //UAT
                                    //string url = "https://staging-rsds.emudhra.com/api/Doc/v1/Signature";

                                    // Production
                                    strXML = @"<?xml version='1.0' encoding='UTF-8'?><SignDocReq version='1.0' ts='" + strDateFormat + "' txn='" + strTransaction + "' clientID='4851480667' keyID='iLaIhdjP' accessKeyhash='" + strGeneratesha256_hash + "' sessionId=''><Docs><Doc id='1' docType='pdf'><DocData>" + file + "</DocData><Signatures certified='0' appearanceId='1' position='A-XY|236,372,396,472' locationText='' reasonText='SureshDubey' tick='1' border='1'>U3VyZXNoRHViZXk=</Signatures></Doc></Docs></SignDocReq>";

                                    //UAT
                                    //strXML = @"<?xml version='1.0' encoding='UTF-8'?><SignDocReq version='1.0' ts='" + strDateFormat + "' txn='" + strTransaction + "' clientID='6348283573' keyID='60ySQ2qy' accessKeyhash='" + strGeneratesha256_hash + "' sessionId=''><Docs><Doc id='1' docType='pdf'><DocData>"+ file + "</DocData><Signatures certified='0' appearanceId='1' position='A-XY|236,372,396,472' locationText='' reasonText='SureshDubey' tick='1' border='1'>U3VyZXNoRHViZXk=</Signatures></Doc></Docs></SignDocReq>";
                                    string str = postXMLData(url, HttpUtility.UrlEncode(strXML));

                                    // Creating signed document

                                    //Convert Base 64 to pdf
                                    XmlDocument xmlDoc = new XmlDocument();
                                    xmlDoc.LoadXml(str);
                                    XmlNodeList nodes = xmlDoc.SelectNodes("/SignDocResp/DocSignatures/DocSignature");
                                    foreach (XmlNode item in nodes)
                                    {
                                        string res = item.InnerText;
                                        byte[] bytes = Convert.FromBase64String(res);
                                        System.IO.FileStream stream = new FileStream(@"D:\DigitalSignature\signedDocument\" + sFileName + ".pdf", FileMode.CreateNew);
                                        System.IO.BinaryWriter writer = new BinaryWriter(stream);
                                        writer.Write(bytes, 0, bytes.Length);
                                        writer.Close();
                                    }


                                    Response.ClearHeaders();                             
                                    iUploadedCnt += 1;
                                }
                            }
                        }
                    }
                    lblUploadStatus.Text = "<b>" + iUploadedCnt + "</b> file(s) Uploaded.";
                    lblFailedStatus.Text = "<b>" + iFailedCnt +   "</b> duplicate file(s) could not be uploaded.";
                }
                    else lblUploadStatus.Text = "Max. 10 files allowed.";
            }
            else lblUploadStatus.Text = "No files selected.";
        }

        public string postXMLData(string destinationUrl, string requestXml)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(requestXml);
            //request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentType = "application/xml;";
            request.Accept = "application/xml";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                return responseStr;
            }
            return null;
        }
    }
}