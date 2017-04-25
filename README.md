## WaveConnector
An Simple Utility that lets you import external data from OData V2 API to Salesforce Wave Analytics.

###Instructions:

* Register for a Wave enabled developer account [here](https://developer.salesforce.com/promotions/orgs/wave-de)
* Download the WaveConnector utility from [here](https://github.com/saiteja09/WaveConnector/raw/master/WaveConnector/bin/Release/WaveConnector.exe)
* If you need a OData service for your databases, you can use [Progress DataDirect Cloud](https://www.progress.com/cloud-data-integration). You can visit this [page](https://www.progress.com/odata) to know about the supported data sources by Progress DataDirect Cloud.
* You can refer to this [tutorial](https://github.com/saiteja09/Integrate2016/tree/master/DataDirect%20Cloud) on how you can use DataDirect Cloud to produce OData for your datasource.
* Run the WaveConnector.exe and you should see the below screen. Login with your Salesforce credentials and your security Token.

![Login form](https://s3.amazonaws.com/progressintegrate2016/Login.PNG)

* On the next form, paste your base OData URL for your datasource and choose the authentication type (Basic Auth or No auth).

![odata conf form](https://s3.amazonaws.com/progressintegrate2016/odataconf.PNG)

* For DataDirect Cloud users, choose Basic auth as your authentication type and enter your DataDirect cloud credentials on the form. If	your OData service is publicly accessible with out requiring any authentication, then choose No Auth.

* Click on Test Connection, If everything is correctly configured, your connection should be a success. Click on Continue button to go to next form.

* On the next form, you can either choose an entity that you want to import to Wave Analytics or write your OData URL with custum filters.
* If you choose entity from dropdown, all the data in the entity will be uploaded. If you choose to write your own OData URL, please note that currently $filter and $top are the only parameters that are supported. Based on your filters, the data will be uploaded to Wave Analytics.

![upload form](https://s3.amazonaws.com/progressintegrate2016/Uplaod.PNG)

* Enter the datasetname that you would like to have for this data in Wave Analytics and click on Upload to start the upload. Once the upload is finished, you should be able to see the dataset in wave analytics.
* If the dataset is huge, it might take a bit of time and to check the progress, go to Wave Analytics -> Click on gear icon on top right -> DataMonitor. Here you should be able to track the progress.

