﻿using AppLabRedes.Scripts.MyScripts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;


//http://www.aspsnippets.com/Articles/Create-dynamic-textbox-using-JavaScript-in-ASP.Net.aspx
//http://stackoverflow.com/questions/3464498/pass-c-sharp-asp-net-array-to-javascript-array
namespace AppLabRedes.Lab
{
    public partial class EditLab : System.Web.UI.Page
    {
        /// <summary>
        /// Id Lab to edit
        /// </summary>
        int IDToEdit;


        protected void Page_Load(object sender, EventArgs e)
        {
            IDToEdit = Convert.ToInt16(Request.QueryString["idLab"]);
            //just in post back binds all the text
            if (!IsPostBack)
                BindText(IDToEdit);

        }
        /// <summary>
        /// Binds all text of the lab
        /// </summary>
        /// <param name="idLab"></param>
        protected void BindText(int idLab)
        {
            //gets lab types
            DataTable dt = SqlCode.PullDataToDataTable(
                " select t.id as id, t.type" +
                " from tblLabs as l , tblLabType as t , tblTypes_Labss as tl " +
                " where l.Id=tl.IdLab and t.Id = tl.IdType and l.id=" + idLab + "");
            //gets all from the lab
            DataTable dt1 = SqlCode.PullDataToDataTable("select * from tblLabs where id='" + idLab + "'");
            //gets all the types
            DataTable dt2 = SqlCode.PullDataToDataTable("select * from tblLabType");
            //sets the information
            txtLabName.Text = Convert.ToString(dt1.Rows[0]["name"]);
            txtNumPods.Text = Convert.ToString(dt1.Rows[0]["numPods"]);
            txtDescription.Text = Convert.ToString(dt1.Rows[0]["description"]);

            //creates the types field
            foreach (DataRow row in dt2.Rows) // Loop over the items.
            {
                String tp = Convert.ToString(row["type"]);
                String tId = Convert.ToString(row["id"]);
                ListItem lst = new ListItem(tp, tId);
                ckbTypes.Items.Add(lst);
            }
            //selects the types of the lab
            foreach (ListItem listItem in ckbTypes.Items)
            {
                foreach (DataRow row in dt.Rows) // Loop over the items.
                {
                    string iddd = Convert.ToString(row["id"]);
                    if (listItem.Value == iddd)
                    {
                        listItem.Selected = true;
                    }
                }
            }
        }
        /// <summary>
        /// Button to update all information.Updates the Lab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUpdateLab_Click(object sender, EventArgs e)
        {
            //gets the new information
            string labName = txtLabName.Text;
            int numPods = Convert.ToInt16(txtNumPods.Text);
            string description = txtDescription.Text;
                        //if all fields are filled
            if (txtLabName.Text != "" && txtNumPods.Text != "" && txtDescription.Text != "")
            {
                //removes the dependencies
                RemoveType_Lab(IDToEdit);
                //add new dependencie
                addType_Lab(IDToEdit);
                //updates the table
                UpdateLab(IDToEdit, labName, numPods, description);
                //redirects
                System.Threading.Thread.Sleep(3000);
                Response.Redirect("~/Lab/Labs.aspx");
            }
        }
        /// <summary>
        /// updates the table Lab
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="numPods"></param>
        /// <param name="description"></param>
        private void UpdateLab(int id, string name, int numPods, string description)
        {

            String strConn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection openCon = new SqlConnection(strConn))
            {
                //command
                string saveLab = " update tblLabs set name =@name,numPods = @numPods, description = @description where id=@id"; ;
                using (SqlCommand command = new SqlCommand(saveLab, openCon))
                {
                    //new id
                    int maxId = SqlCode.SelectForINT("Select Max(id)+1 From tblLabType");
                    //parameters
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@numPods", numPods);
                    command.Parameters.AddWithValue("@description", description);
                    try
                    {
                        //opens the connection
                        openCon.Open();
                        int recordsAffected = command.ExecuteNonQuery();

                    }
                    catch (SqlException ex)
                    {
                        //error message
                        txtOutput.Text = ex.Message + "@UpdateData";
                    }
                    finally
                    {
                        //closes the connection
                        openCon.Close();
                    }
                    command.Parameters.Clear();
               }
            }
        }
        /// <summary>
        /// adds the Type - Lab dependencies
        /// </summary>
        /// <param name="idLab"></param>
        private void addType_Lab(int idLab)
        {
            String strConn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection openCon = new SqlConnection(strConn))
            {
                //command
                string saveTypes_Lab = " insert into tblTypes_Labss (IdType,IdLab) VALUES (@idType,@idLab)"; ;
                //runs the types
                foreach (ListItem listItem in ckbTypes.Items)
                {
                    //gets the selected ones
                    if (listItem.Selected == true)
                    {
                        //gets the id of the type
                        int idType = Convert.ToInt16(listItem.Value);
                        using (SqlCommand command = new SqlCommand(saveTypes_Lab, openCon))
                        {
                            //command parameters
                            command.Parameters.AddWithValue("@idType", idType);
                            command.Parameters.AddWithValue("@idLab", idLab);
                            try
                            {
                                //opens the connection
                                openCon.Open();
                                int recordsAffected = command.ExecuteNonQuery();
                            }
                            catch (SqlException ex)
                            {
                                //error message
                                txtOutput.Text = ex.Message + "@InsertData";
                            }
                            finally
                            {
                                //closes the connection
                                openCon.Close();
                            }
                            command.Parameters.Clear();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes t
        /// </summary>
        /// <param name="idLabb"></param>
        private void RemoveType_Lab(int idLabb)
        {
            String strConn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection openCon = new SqlConnection(strConn))
            {
                //commands
                string saveTypes_Lab = " delete from tblTypes_Labss where idLab= @idLab"; ;
                using (SqlCommand command = new SqlCommand(saveTypes_Lab, openCon))
                {
                    //command parameters
                    command.Parameters.AddWithValue("@idLab", idLabb);
                    try
                    {
                        //opens the connection
                        openCon.Open();
                        int recordsAffected = command.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        //error message
                        txtOutput.Text = ex.Message + "@DeleteData";
                    }
                    finally
                    {
                        //opens the connection
                        openCon.Close();
                    }
                    command.Parameters.Clear();
                }
            }
        }
    }
}