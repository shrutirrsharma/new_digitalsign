<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateDigitalSignature.aspx.cs" Inherits="DigitalSignature.CreateDigitalSignature" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Digital Signature</title>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js"></script>    
</head>
<body>
    <form id="form1" runat="server">
        <div id="divFile">
            <h3>Digital Signature</h3>
            <p>
                <asp:FileUpload ID="fileUpload" multiple="true" runat="server" accept=".pdf"/>
            </p>
            <p>
                <asp:Button ID="btUpload" Text ="Upload Files" OnClick="Upload_Files" runat="server"/>
            </p>
            <p><asp:label id="lblFileList" runat="server"></asp:label></p>
            <p><asp:Label ID="lblUploadStatus" runat="server"></asp:Label></p>
            <p><asp:Label ID="lblFailedStatus" runat="server"></asp:Label></p>
        </div>
    </form>
</body>
    <script>
        $('#btUpload').click(function () {
            if (fileUpload.value.length == 0) {
                alert('No files selected.');
                return false;
            }
        });
    </script>
</html>
