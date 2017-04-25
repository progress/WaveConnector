using Microsoft.Data.Edm;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows;
using System.Xml;
using WaveConnector.SFDC;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.IO;
using CsvHelper;
using System.Threading.Tasks;
using System.Windows.Threading;


namespace WaveConnector
{
    /// <summary>
    /// Interaction logic for ODataEntitySelect.xaml
    /// </summary>
    public partial class ODataEntitySelect : Window
    {
        private SFDC.SforceService SFDCBinding;
        private ODataClientSettings settings;
        private ODataClient client;
        private IEdmModel response;
        private Dictionary<string, string> metadata;
        private Object lockThis = new Object();
        private int partNumber;
        private string[] properties;
        public ODataEntitySelect(ODataClientSettings settings, ODataClient client, SforceService SFDCBinding)
        {
            partNumber = 1;
            InitializeComponent();
            this.settings = settings;
            this.client = client;
            this.SFDCBinding = SFDCBinding;

            //Set baseURL Configured in previous step
            txtBaseURL.Text = settings.BaseUri.ToString();
            
        }

        private async void rbChooseEntity_Checked(object sender, RoutedEventArgs e)
        {
            cbEntities.IsEnabled = true;
            stOData.IsEnabled = false;

            //Add Entity Names to ComboBox if they are not yet loaded.
            if(!cbEntities.HasItems)
            {
                //IEdmModel response = null;
                try
                {
                    response = await client.GetMetadataAsync<Microsoft.Data.Edm.IEdmModel>();
                    foreach(var entity in response.SchemaElements)
                    {
                        
                        if (entity.GetType().ToString() != "Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics.CsdlSemanticsEntityContainer" && entity.GetType().ToString() != "Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics.CsdlSemanticsComplexTypeDefinition")
                        {
                            cbEntities.Items.Add(((IEdmEntityType)entity).Name);
                        }
                    }
                }
                catch(Exception ex)
                {
                    System.Windows.MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private async void rbOdata_Checked(object sender, RoutedEventArgs e)
        {
            cbEntities.IsEnabled = false;
            stOData.IsEnabled = true;
            if (!cbEntities.HasItems)
            {
                //IEdmModel response = null;
                try
                {
                    response = await client.GetMetadataAsync<Microsoft.Data.Edm.IEdmModel>();
                    foreach (var entity in response.SchemaElements)
                    {
                        if (entity.GetType().ToString() != "Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics.CsdlSemanticsEntityContainer" && entity.GetType().ToString() != "Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics.CsdlSemanticsComplexTypeDefinition")
                        {
                            cbEntities.Items.Add(((IEdmEntityType)entity).Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private async void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            btnUpload.IsEnabled = false;
            txtDataSetName.IsEnabled = false;
            txtFilters.IsEnabled = false;

            status.Text = "Fetching Metadata...";
            status.Visibility = Visibility.Visible;

            if(String.IsNullOrWhiteSpace(txtDataSetName.Text))
            {
                System.Windows.MessageBox.Show("DataSet Name cannot be empty");
                txtDataSetName.IsEnabled = true;
                btnUpload.IsEnabled = true;
                return;
            } else if (String.IsNullOrWhiteSpace(txtFilters.Text) && (rbOdata.IsChecked ==true ))
            {
                System.Windows.MessageBox.Show("Incomplete OData URL. Specify a table and/or any filters in the URL.");
                txtDataSetName.IsEnabled = true;
                btnUpload.IsEnabled = true;
                return;
            }

            sObject sobj = new sObject();
            sobj.type = "InsightsExternalData";
            XmlElement[] fields = new XmlElement[6];
            XmlDocument doc = new XmlDocument();

            string metadata = generateMetaDatafromOData();
            if(metadata == null)
            {
                System.Windows.MessageBox.Show("The specified table cannot be accessed from the endpoint you provided. Please verify if you have OData enabled for that table or check your OData endpoint.");
                txtDataSetName.IsEnabled = true;
                btnUpload.IsEnabled = true;
                return;
            }

            status.Text = "Uploading Metadata...";

            fields[0] = doc.CreateElement("Format");
            fields[0].InnerText = "Csv";

            fields[1] = doc.CreateElement("EdgemartAlias");
            fields[1].InnerText = txtDataSetName.Text;

            fields[2] = doc.CreateElement("MetadataJson");
            fields[2].InnerText = metadata;

            fields[3] = doc.CreateElement("Operation");
            fields[3].InnerText = "Overwrite";

            fields[4] = doc.CreateElement("Action");
            fields[4].InnerText = "None";

            sobj.Any = fields;

            sObject[] metaList = new sObject[1];
            metaList[0] = sobj;

            SaveResult[] results = SFDCBinding.create(metaList);
            string rowid = getSFDCROWID(results);
            

            status.Text = "Initiating Data transfer ...";
            await startUpload(rowid);
            
        }

        private string getEntityName()
        {
          
            if(rbChooseEntity.IsChecked == true)
            {
                return cbEntities.Text;
            } else if (rbOdata.IsChecked == true)
            {
               
                string tabledetails = txtFilters.Text.Split('/')[1];
                string tablename = tabledetails.Split('?')[0];
                return tablename;
            }
            return null;
        }

        private string generateMetaDatafromOData()
        {

            string entityName = getEntityName();
            CultureInfo cInfo = new CultureInfo("en-us");
            PluralizationService pService = PluralizationService.CreateService(cInfo);
            entityName = pService.Singularize(entityName);

            MetadataWrapper.Rootobject meta = new MetadataWrapper.Rootobject();
            meta.fileFormat = new MetadataWrapper.Fileformat();
            Utils.helper helperObj = new Utils.helper();
            metadata = new Dictionary<string, string>();
            //Setting FileFormat
            meta.fileFormat.charsetName = "UTF-8";
            meta.fileFormat.fieldsEnclosedBy = "\"";
            meta.fileFormat.fieldsDelimitedBy = ",";
            meta.fileFormat.numberOfLinesToIgnore = 0;

            //Objects 
            meta.objects = new MetadataWrapper.Object[1];
            meta.objects[0] = new MetadataWrapper.Object();
            meta.objects[0].connector = "WaveConnector";
            //meta.objects[0].fullyQualifiedName = entityName;
            meta.objects[0].label = entityName;
            meta.objects[0].name = entityName;
            foreach (var entity in response.SchemaElements)
            {
                if (entity.Name.ToLower() == entityName.ToLower())
                {
                    IEdmEntityType entityObj = (IEdmEntityType)entity;
                    meta.objects[0].fullyQualifiedName = entity.Namespace + "." + entityName;
                    //Get Primary Keys List
                    List<IEdmStructuralProperty> keyProperties = (List<IEdmStructuralProperty>)entityObj.DeclaredKey;
                    List<string> primary_keys = new List<string>();
                    foreach (IEdmStructuralProperty property in keyProperties)
                    {
                        primary_keys.Add(property.Name);
                    }


                    //Get All Fields
                    List<IEdmProperty> entityProperties = (List<IEdmProperty>)entityObj.DeclaredProperties;
                    int propertyCount = 0;
                    foreach (IEdmProperty property in entityProperties)
                    {
                        if (property.GetType().ToString() != "Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics.CsdlSemanticsNavigationProperty")
                        {
                            propertyCount++;
                        }
                    }
                    properties = new string[propertyCount];
                    meta.objects[0].fields = new MetadataWrapper.Field[propertyCount];
                    int index = 0;
                    foreach (IEdmProperty property in entityProperties)
                    {
                        if (property.GetType().ToString() != "Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics.CsdlSemanticsNavigationProperty")
                        {
                            //Setting up field Label, name and FullyqualifiedName
                            meta.objects[0].fields[index] = new MetadataWrapper.Field();
                            meta.objects[0].fields[index].fullyQualifiedName = entity.Name + "." + property.Name;
                            meta.objects[0].fields[index].label = property.Name;
                            meta.objects[0].fields[index].name = property.Name;
                            properties[index] = property.Name;

                            //Getting the DataTpe of the field and assosciated properties
                            string typeInfo = ((IEdmTypeReference)property.Type).ToString();
                            string[] fieldproperties = helperObj.getfieldProperties(typeInfo);
                            string SFDCtype = helperObj.getODataToSFDCType(fieldproperties[0]);
                            metadata.Add(property.Name, fieldproperties[0]);
                            //Setting Data Type
                            meta.objects[0].fields[index].type = SFDCtype;

                            //Setting Primary Key
                            if (primary_keys.Contains(property.Name) && primary_keys.Count == 1 && SFDCtype == "Text")
                            {
                                meta.objects[0].fields[index].isUniqueId = true;
                            }
                            else
                            {
                                meta.objects[0].fields[index].isUniqueId = false;
                            }

                            //Setting Precision and Scale based on Data Type
                            if (SFDCtype.Equals("Numeric"))
                            {
                                //Using Max allowable Precision and scale
                                meta.objects[0].fields[index].precision = 18;
                                meta.objects[0].fields[index].scale = 2;
                                meta.objects[0].fields[index].defaultValue = "" + 0;
                            }
                            //Set Max Length as Precision to Text based types
                            else if (SFDCtype.Equals("Text") && !(fieldproperties.Length < 4))
                            {
                                int maxLength = Int32.Parse(fieldproperties[3].Split('=')[1]);

                                if (maxLength > 255)
                                    meta.objects[0].fields[index].precision = maxLength;
                            }

                            //Set DateTime format specified by Wave External API. Defaulting to 'yyyy-mm-dd hh:mm:ss'
                            if (fieldproperties[0] == "Edm.DateTime")
                            {
                                meta.objects[0].fields[index].format = "yyyy-MM-dd HH:mm:ss";
                            }
                            else if (fieldproperties[0] == "Edm.DateTimeOffset")
                            {
                                meta.objects[0].fields[index].format = "yyyy-MM-dd'T'HH:mm:ss'Z'";
                            }

                            index++;
                        }

                        
                    }
                    break;
                }
            }

            //Serialize Object to JSON
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { new Utils.NullPropertiesConverter() });
            string json = serializer.Serialize(meta);
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
        }

        private Dictionary<string, string> getfilters()
        {
            string[] filterquery = txtFilters.Text.Split(new string[] { "?$" }, StringSplitOptions.RemoveEmptyEntries);
            string[] queryOptions = filterquery[1].Split(new string[] { "&$" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> filters = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            
            for(int i =0; i < queryOptions.Length; i++)
            {
                string[] queryOption = queryOptions[i].Split('=');
                filters.Add(queryOption[0], queryOption[1]);
            }
             
            return filters;
        }

        private async Task startUpload(string parentid)
        {


            lock (lockThis)
            {
                partNumber = 1;
            }
            Dictionary<string, string> filters = null;
            if (rbOdata.IsChecked == true)
            {
                filters = getfilters();
            }
           
            string entityName = getEntityName().ToUpper();
            double count = 0;
            if (filters != null)
            {
                if(filters.ContainsKey("top"))
                {
                    count = Double.Parse(filters["top"]);
                }
                else
                {
                    count = await client.For(entityName).Filter(filters["filter"]).Count().FindScalarAsync<int>();
                }
            }
            else
            {
                count = await client.For(entityName).Count().FindScalarAsync<int>();
            }
             


            status.Text = "Fetching and Uploading Data ... 0/" + count;
            int uploadedRecords = 0;

            prgsBar.IsIndeterminate = false;
            prgsBar.Minimum = 0;
            prgsBar.Maximum = count;



            int fetchSize = 1000;
            int maxThreads = 5;
            if (count < fetchSize)
            {
                await UploadData(parentid, fetchSize, 0, entityName, filters);
                processData(parentid);
                status.Visibility = Visibility.Hidden;
                MessageBox.Show("Uploaded " + count + " records successfully");
                btnUpload.IsEnabled = true;
                txtDataSetName.IsEnabled = true;
                txtFilters.IsEnabled = true;
                prgsBar.Value = 0;
            }
            else
            {
                int threads = (int)Math.Ceiling((double)count / fetchSize);
                if (threads > maxThreads)
                {
                    threads = maxThreads;
                }
                int synchedData = 0;
                while (count > synchedData)
                {
                    if(count - synchedData > maxThreads * fetchSize)
                    {
                        threads = maxThreads;
                    }
                    else
                    {
                        threads = (int)Math.Ceiling((double)(count - synchedData) / fetchSize);
                    }
                    Task<int>[] taskSync = new Task<int>[threads];
                    
                    int[] skip = new int[threads];
                    skip[0] = 0;
                    for (int i = 0; i < threads; i++)
                    {
                        if(i !=0)
                        {
                            skip[i] = skip[i-1] + fetchSize;
                        } else if (i ==0)
                        {
                            skip[i] = synchedData;
                        }
                        int temp = skip[i];
                        taskSync[i] = Task.Run(() => UploadData(parentid, fetchSize, temp, entityName, filters));
                        synchedData = fetchSize + synchedData;
                       

                    }
                    
                    List<int> taskCompletedList = new List<int>();
                    while (taskCompletedList.Count != threads)
                    {
                        int index =Task.WaitAny(taskSync);

                        for (int i = 0; i < threads; i++)
                        {
                            if (!taskCompletedList.Contains(taskSync[i].Id) && taskSync[i].Status == TaskStatus.RanToCompletion)
                            {
                                taskCompletedList.Add(taskSync[i].Id);
                                

                                Dispatcher.Invoke(new Action(() =>

                                {
                                    prgsBar.Value = prgsBar.Value + taskSync[i].Result;
                                    uploadedRecords = uploadedRecords + taskSync[i].Result;
                                    status.Text = "Fetching and Uploading Data ..."+ uploadedRecords +"/" + count;
                                }), DispatcherPriority.Background);
                            }
                        }
                    }

                }
                
                processData(parentid);
                status.Visibility = Visibility.Hidden;
                MessageBox.Show("Uploaded " + count + " records successfully");
                btnUpload.IsEnabled = true;
                txtDataSetName.IsEnabled = true;
                txtFilters.IsEnabled = true;
                prgsBar.Value = 0;
            }
        }


        private async Task<int> UploadData(String parentid, int top, int skip, string entityName, Dictionary<string, string> filter)
        {
            string path = await getData(entityName, top, skip, filter);
            prepAndUploadCSV(path, parentid);
            return File.ReadAllLines(path).Length;
        }

        private async Task<string> getData(string entityName, int top, int skip, Dictionary<string, string> filter)
        {
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var complete = Path.Combine(systemPath, "WaveConnector");
            Directory.CreateDirectory(complete);
            IEnumerable<IDictionary<string,object>> products = null;
            if (filter != null && filter.ContainsKey("filter"))
            {
                 products = await client.For(entityName).Filter(filter["filter"]).Skip(skip).Top(top).FindEntriesAsync();
            }
            else
            {
                 products = await client.For(entityName).Skip(skip).Top(top).FindEntriesAsync();
            }
            string filename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid() + ".csv";
            using (TextWriter textWriter = File.CreateText(complete + "\\" + filename))
            {
                var csv = new CsvWriter(textWriter);
                csv.Configuration.Delimiter = ",";
                csv.Configuration.Quote = '"';
                csv.Configuration.TrimFields = true;
                //int i = 0;

                foreach (Dictionary<string, object> record in products)
                {
                    for(int i =0; i < properties.Length; i++)
                    {
                        string value = null;
                        if (record.ContainsKey(properties[i]))
                        {
                            string datatype = metadata[properties[i]];
                            

                            if (datatype.Equals("Edm.DateTime"))
                            {
                                value = Convert.ToDateTime(record[properties[i]]).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                value = (record[properties[i]] == null) ? "" : record[properties[i]].ToString();
                            }
                        }else
                        {
                            value = "";
                        }
                        csv.WriteField(value);
                    }
                    /*foreach (KeyValuePair<string, object> field in record)
                    {
                            string datatype = metadata[field.Key];
                            string value = null;

                            if (datatype.Equals("Edm.DateTime"))
                            {
                                value = Convert.ToDateTime(field.Value).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                value = (field.Value == null) ? "" : field.Value.ToString();
                            }
                       

                        csv.WriteField(value);
                    }*/
                    csv.NextRecord();
                }
            }

            return (complete + "\\" + filename);
        }

        private void prepAndUploadCSV(string path, string parentid)
        {
            long read = 0;
            int r = -1;
            long bytesToRead = new System.IO.FileInfo(path).Length;
            int bufferSize = 10 * 1024 * 1024;
            byte[] buffer = new byte[bufferSize];
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                while (read <= bytesToRead && r != 0)
                {
                    read += (r = stream.Read(buffer, 0, bufferSize));
                    if (r == 0)
                    {
                        break;
                    }
                    uploadDatatoSFDC(buffer, parentid);
                }
            }
        }

        private void uploadDatatoSFDC(byte[] buffer, string parentid)
        {
            string rowid;
            sObject sobj = new sObject();
            sobj.type = "InsightsExternalDataPart";

            XmlElement[] fields = new XmlElement[3];
            XmlDocument doc = new XmlDocument();

            string data = System.Text.Encoding.Default.GetString(buffer);
            data = data.Replace("\0", String.Empty);

            fields[0] = doc.CreateElement("DataFile");
            fields[0].InnerText = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));

            fields[1] = doc.CreateElement("PartNumber");
            lock (lockThis)
            {
                fields[1].InnerText = partNumber.ToString();
                partNumber++;
            }
            fields[2] = doc.CreateElement("InsightsExternalDataId");
            fields[2].InnerText = parentid;

            sobj.Any = fields;

            sObject[] metaList = new sObject[1];
            metaList[0] = sobj;

            SaveResult[] results = SFDCBinding.create(metaList);
            rowid = getSFDCROWID(results);

        }

        private void processData(string parentid)
        {
            sObject sobj = new sObject();
            XmlElement[] fields = new XmlElement[1];
            XmlDocument doc = new XmlDocument();
            sobj.type = "InsightsExternalData";
            sobj.Id = parentid;
            fields[0] = doc.CreateElement("Action");
            fields[0].InnerText = "Process";
            sobj.Any = fields;
            sObject[] metaList = new sObject[1];
            metaList = new sObject[1];
            metaList[0] = sobj;
            SaveResult[] results = SFDCBinding.update(metaList);
            string rowid = getSFDCROWID(results);
        }

        private string getSFDCROWID(SaveResult[] results)
        {

            string rowid = null;
            for (int j = 0; j < results.Length; j++)
            {
                if (results[j].success)
                {
                    rowid = results[j].id;
                    Console.Write("\nSuccessful with an ID of: "
                                    + results[j].id);
                }
                else
                {
                    for (int i = 0; i < results[j].errors.Length; i++)
                    {
                        Error err = results[j].errors[i];
                        Console.WriteLine("Errors were found on item " + j.ToString());
                        Console.WriteLine("Error code is: " + err.statusCode.ToString());
                        Console.WriteLine("Error message: " + err.message);
                    }
                }
            }
            return rowid;
        }
    }
}


