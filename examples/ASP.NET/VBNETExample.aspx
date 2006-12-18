<%@ Page Language="VB" Debug="true" %>
<%@ import Namespace="System.Data" %>
<%@ import Namespace="FirebirdSql.Data.Firebird" %>
<script runat="server">

    '
    ' Firebird .NET Data Provider - Firebird managed data provider for .NET and Mono
    '
    ' Author : Andrew C. Goodall
    '
    
    Dim public shared myConnectionString As String
    Dim public shared myConnection As FbConnection
    Dim public shared myTxn As FbTransaction
    Dim public shared selectCmd As String
    Dim public shared mycommand As FbCommand
    Dim public shared myReader As FbDataReader
    Dim shared myXMLfile As String = "data.xml"
    
    Sub Connect()
    
        ' Use Pooling=False so that connections are physically dropped immediately when connection is closed
		' Set the ServerType to 1 for connect to the embedded server
        myConnectionString = "Database=C:\EMPLOYEE.FDB;User=SYSDBA;Password=dark_heart;Dialect=3;Server=localhost;Pooling=False;ServerType=0"
    
        myConnection = new FbConnection(myConnectionString)
        myConnection.Open()
    
        myTxn = myConnection.BeginTransaction()
    
    End Sub
    
    Sub Page_Load(Sender As Object, E As EventArgs)
    
       If Not (IsPostBack) 'Start postback
    
            Connect()
    
            selectCmd = "SELECT * FROM EMPLOYEE"
    
            myCommand   = new FbCommand(selectCmd, myConnection, myTxn)
            myReader    = myCommand.ExecuteReader()
    
            ' in this case selectsystem was an asp dropdown list in the html section
            cboSelectEmployee.DataSource = myReader
            cboSelectEmployee.DataBind()
    
            ' dispose objects
            myReader.close()
            myTxn.Dispose()
            myCommand.Dispose()
            myConnection.Close()
    
       End If 'End postback
    
    End Sub
    
    
    Sub BindGrid()
    
        Dim dataAdapter As FbDataAdapter
        Dim DS As New DataSet()
    
        Connect()
    
        dataAdapter = new FbDataAdapter()
    
        ' Select the salary history of the selected employee
        selectCmd = "SELECT CHANGE_DATE, UPDATER_ID, OLD_SALARY, PERCENT_CHANGE, NEW_SALARY, 'A' AS A FROM SALARY_HISTORY WHERE EMP_NO = @EMP_NO "
        myCommand = new FbCommand(selectCmd, myConnection, MyTxn)
    
        myCommand.parameters.add("@EMP_NO", FbDbType.SmallInt)
        myCommand.parameters("@EMP_NO").value = cboSelectEmployee.SelectedItem.Value
    
        Try
            dataAdapter.SelectCommand = myCommand
            dataAdapter.Fill(DS, "TempInfo")  'filldataset
    
            mydatagrid.DataSource = DS.Tables("TempInfo")
            mydatagrid.DataBind()
    
            dataAdapter.Dispose()
            myTxn.Dispose()
            myCommand.Dispose()
            myConnection.Close()
    
            Message2.Style("color") = "green"
            Message2.InnerHtml = "Matching Records = " & myDataGrid.Items.Count
    
        Catch Exp As FbException
            Message.Style("color") = "red"
            Message.InnerHtml = "ERROR:" & Exp.Message & "<br>" & selectcmd
            myTxn.Dispose()
            myCommand.Dispose()
            myConnection.Close()
            Exit Sub
        End Try
    
        ' Write Dataset to XML file on server to use later as desired - e.g. for datagrid sorting or
        ' re-querying with extra paramters
    
        DS.WriteXml(Server.MapPath(myXMLfile), XmlWriteMode.WriteSchema)
    
    End Sub
    
    Sub BindXML()
    
        Dim myDataSet as New DataSet()
        Dim fsReadXml As New System.IO.FileStream(Server.MapPath(myXMLfile), System.IO.FileMode.Open)
        Dim myView As New DataView()
    
        Try
            myDataSet.ReadXml(fsReadXml)
            myView = myDataSet.Tables(0).DefaultView
    
            mydatagrid.DataSource = myView
            mydatagrid.DataBind()
    
            Message2.Style("color") = "green"
            Message2.InnerHtml      = "Matching Records = " & myDataGrid.Items.Count
        Catch ex As Exception
            Message.Style("color")  = "red"
            Message.InnerHtml       = ex.message.ToString()
        Finally
            fsReadXml.Close()
        End Try
    
    End Sub
    
    Sub GetNew(Sender As Object, E As EventArgs)
    
        BindGrid()
    
    End Sub
    
    Sub UseSaved(Sender As Object, E As EventArgs)
    
        BindXML()
    
    End Sub
    
    Sub MyDataGrid_Edit(Sender As Object, E As DataGridCommandEventArgs)
    
        MyDataGrid.EditItemIndex = E.Item.ItemIndex
        BindGrid()
    
    End Sub
    
    Sub MyDataGrid_Cancel(Sender As Object, E As DataGridCommandEventArgs)
    
        MyDataGrid.EditItemIndex = -1
        BindGrid()
    
    End Sub
    
    Sub MyDataGrid_Update(Sender As Object, E As DataGridCommandEventArgs)
    
        Dim UpdateCmd As String
    
        Connect()
    
        Updatecmd = "UPDATE SALARY_HISTORY SET OLD_SALARY = @OLD_SALARY, PERCENT_CHANGE = @PERCENT_CHANGE WHERE EMP_NO = @EMP_NO AND CHANGE_DATE = @CHANGE_DATE AND UPDATER_ID = @UPDATER_ID"
    
        MyCommand = New FbCommand(updateCmd, MyConnection, myTxn)
    
        myCommand.parameters.add("@OLD_SALARY", FbDbType.Double)
        myCommand.parameters("@OLD_SALARY").value = Double.Parse(GetCellValue(e.Item.Cells(4)))
    
        myCommand.parameters.add("@PERCENT_CHANGE", FbDbType.Double)
        myCommand.parameters("@PERCENT_CHANGE").value = Double.Parse(GetCellValue(e.Item.Cells(5)))
    
        myCommand.parameters.add("@EMP_NO", FbDbType.SmallInt)
        myCommand.parameters("@EMP_NO").value = cboSelectEmployee.SelectedItem.Value
    
        myCommand.parameters.add("@CHANGE_DATE", FbDbType.TimeStamp)
        myCommand.parameters("@CHANGE_DATE").Value = DateTime.Parse(E.Item.Cells(2).Text)
    
        myCommand.parameters.add("@UPDATER_ID", FbDbType.VarChar)
        myCommand.parameters("@UPDATER_ID").Value = E.Item.Cells(3).Text
    
        ' Note: You have to do a Commit unlike SQL Client or OLEDB Client. I Also do a rollback if an error is found
        Try
            MyCommand.ExecuteNonQuery()
            myTxn.Commit()
            myTxn.Dispose()
            myCommand.Dispose()
    
            Message.Style("color") = "Green"
            Message.InnerHtml = "<b>Record has been updated.</b><br>"
        Catch Exp As FbException
            myTxn.Rollback()
            myTxn.Dispose()
            myCommand.Dispose()
    
            Message.Style("color") = "red"
            Message.InnerHtml = Exp.Message
        Finally
            myConnection.Close()
        End Try
    
        MyDataGrid.EditItemIndex = -1
        BindGrid()
    
    End Sub
    
    Function GetCellValue(Cell As TableCell) AS String
    
        dim tb As TextBox
    
        tb = Cell.Controls(0)
    
        return tb.Text
    
    End Function
    
    Sub MyDataGrid_ItemDataBound(Sender As Object, E As DataGridItemEventArgs)
    
        If (e.Item.ItemType = ListItemType.EditItem) Then
            Dim i As Integer
            For i = 0 To e.Item.Controls.Count-1
                Try
                    If (e.Item.Controls(i).Controls(0).GetType().ToString() = "System.Web.UI.WebControls.TextBox") Then
                        Dim tb As TextBox
    
                        tb = e.Item.Controls(i).Controls(0)
                        tb.Text = Server.HtmlDecode(tb.Text)
                    End If
                Catch
    
                End Try
            Next
        End If
    
    End Sub
    
    Sub MyDataGrid_Delete(Sender As Object, E As DataGridCommandEventArgs)
    
        Dim DeleteCmd As String
    
        Connect()
    
        Deletecmd = "DELETE FROM SALARY_HISTORY WHERE EMP_NO = @EMP_NO AND CHANGE_DATE = @CHANGE_DATE AND UPDATER_ID = @UPDATER_ID"
    
        MyCommand = New FbCommand(DeleteCmd, MyConnection, myTxn)
    
        myCommand.parameters.add("@EMP_NO", FbDbType.SmallInt)
        myCommand.parameters("@EMP_NO").value = cboSelectEmployee.SelectedItem.Value
    
        myCommand.parameters.add("@CHANGE_DATE", FbDbType.TimeStamp)
        myCommand.parameters("@CHANGE_DATE").Value = DateTime.Parse(E.Item.Cells(2).Text)
    
        myCommand.parameters.add("@UPDATER_ID", FbDbType.VarChar)
        myCommand.parameters("@UPDATER_ID").Value = E.Item.Cells(3).Text
    
        'Note: You have to do a Commit unlike SQL Client or OLEDB Client. I Also do a rollback if an error is found
        Try
            MyCommand.ExecuteNonQuery()
    
            myTxn.Commit()
    
            myTxn.Dispose()
            myCommand.Dispose()
    
            Message.Style("color") = "Green"
            Message.InnerHtml = "<b>Record has been Deleted.</b><br>"
        Catch Exp As FbException
            myTxn.Rollback()
    
            myTxn.Dispose()
            myCommand.Dispose()
    
            Message.Style("color") = "red"
            Message.InnerHtml = Exp.Message
        Finally
            myConnection.Close()
        End Try
    
        MyDataGrid.EditItemIndex = -1
        BindGrid()
    
    End Sub
    
    Sub MyDataGrid_ItemCreated(objSource As object, objArgs As DataGridItemEventArgs)
    
        Dim myDeleteButton As TableCell
    
        myDeleteButton = objArgs.Item.Cells(0)
        myDeleteButton.Attributes.Add("onclick", "return confirm('Are you sure you want to delete?');")
    
    End Sub

</script>
<html>
<head>
</head>
<body>
    <center>
        <form id="myform" runat="server">
            <asp:DropDownList id="cboSelectEmployee" runat="server" BackColor="Azure" DataValueField="Emp_No" DataTextField="First_Name"></asp:DropDownList>
            <p>
                <asp:Button id="cmdNew" onmouseover="this.style.backgroundColor='lightblue';this.style.fontWeight='b&#13;&#10;&#9;&#9;old'" onclick="GetNew" onmouseout="this.style.backgroundColor='buttonface';this.style.fontWeight='n&#13;&#10;&#9;&#9;ormal'" runat="server" Width="100px" Font-Names="Arial" Font-Size="X-Small" ForeColor="Black" Height="25px" Text="New Search"></asp:Button>
                &nbsp; 
                <asp:Button id="cmdSaved" onmouseover="this.style.backgroundColor='gold';this.style.fontWeight='bold'" onclick="UseSaved" onmouseout="this.style.backgroundColor='buttonface';this.style.fontWeight='n&#13;&#10;&#9;&#9;ormal'" runat="server" Width="100px" Font-Names="Arial" Font-Size="X-Small" ForeColor="Black" Height="25px" Text="Use Current"></asp:Button>
            </p>
            <p>
                <span id="Message" runat="server" enableviewstate="false"></span>
                <br />
            </p>
            <div id="Layer1" title="Salary History" style="OVERFLOW: scroll; WIDTH: 980px; POSITION: relative; HEIGHT: 300px">
                <asp:datagrid id="mydatagrid" runat="server" BackColor="White" Font-Names="Verdana" Font-Size="X-Small" AutoGenerateColumns="False" BorderStyle="None" BorderWidth="1px" BorderColor="#999999" CellPadding="3" GridLines="Vertical" OnItemCreated="MyDataGrid_ItemCreated" OnEditCommand="MyDataGrid_Edit" OnDeleteCommand="MyDataGrid_Delete" OnCancelCommand="MyDataGrid_Cancel" OnUpdateCommand="MyDataGrid_Update" OnItemDataBound="MyDataGrid_ItemDataBound">
                    <FooterStyle forecolor="Black" backcolor="#CCCCCC"></FooterStyle>
                    <HeaderStyle font-bold="True" horizontalalign="Center" borderwidth="2px" forecolor="White" borderstyle="Solid" bordercolor="Black" backcolor="#000084"></HeaderStyle>
                    <PagerStyle horizontalalign="Center" forecolor="Black" backcolor="#999999" mode="NumericPages"></PagerStyle>
                    <SelectedItemStyle font-bold="True" forecolor="White" backcolor="#008A8C"></SelectedItemStyle>
                    <AlternatingItemStyle backcolor="Gainsboro"></AlternatingItemStyle>
                    <ItemStyle borderwidth="2px" forecolor="Black" borderstyle="Solid" bordercolor="Black" backcolor="#EEEEEE"></ItemStyle>
                    <Columns>
                        <asp:ButtonColumn Text="Delete" CommandName="Delete"></asp:ButtonColumn>
                        <asp:EditCommandColumn ButtonType="LinkButton" UpdateText="Update" HeaderText="Edit" CancelText="Cancel" EditText="Edit"></asp:EditCommandColumn>
                        <asp:BoundColumn DataField="CHANGE_DATE" ReadOnly="True" HeaderText="Change Date"></asp:BoundColumn>
                        <asp:BoundColumn DataField="UPDATER_ID" ReadOnly="True" HeaderText="Updater"></asp:BoundColumn>
                        <asp:BoundColumn DataField="OLD_SALARY" HeaderText="Old Salary"></asp:BoundColumn>
                        <asp:BoundColumn DataField="PERCENT_CHANGE" HeaderText="Percent Change"></asp:BoundColumn>
                        <asp:BoundColumn DataField="NEW_SALARY" ReadOnly="True" HeaderText="New Salary"></asp:BoundColumn>
                    </Columns>
                </asp:datagrid>
            </div>
            <span id="Message2" runat="server" enableviewstate="false"></span>
        </form>
    </center>
</body>
</html>
