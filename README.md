What is Andy X Connect?
============

Andy X is an open-source distributed streaming platform designed to deliver the best performance possible for high-performance data pipelines, streaming analytics, streaming between microservices and data integrations. Andy X Connect is an open source distributed platform for change data capture. Start it up, point it at your databases, and your apps can start responding to all of the inserts, updates, and deletes that other apps commit to your databases. Andy X Connect is durable and fast, so your apps can respond quickly and never miss an event, even when things go wrong.

<b>Andy X Connect works only with version 2 or later of Andy X.</b>

## Get Started

Follow the [Getting Started](https://andyx.azurewebsites.net/) instructions how to run Andy X.

For local development and testing, you can run Andy X within a Docker container, for more info click [here](https://hub.docker.com/u/buildersoftdev)

After you run Andy X you will have to configure Andy X Connect. Configuring this adapter is quite easy only to json files need to be configured <b>sqlconnection_config.json</b> and <b>xnode_config.json</b>
> <b>sqlconnection_config.json</b> - SQL Connection Configuration file, it's the file where we specify tables which this adapter will check the changes and produce them.

> <b>xnode_config.json</b> - Andy X Configuration File, it's the file where we specify the connection with Andy X.

### Sql Configuration File
Below is an example of configuration file, this file should be saved into config directory of Andy X Connect before running this service.

	{
		"ConnectionString": "Data Source=localhost;Initial Catalog={databaseName/master};Integrated Security=False;User Id=sa;Password=YourStrong!Passw0rd;MultipleActiveResultSets=True",
		"Databases":[
			{
				"Name": "{databaseName}",
				"Tables": [
					{
						"Name": "{tableName}",
						"Insert": true,
						"Update": true,
						"Delete": true
					},
					{
						"Name": "{tableName}",
						"Insert": true,
						"Update": true,
						"Delete": false
					}
				]
			}
		]
	}

### Andy X Configuration File
Below is an example of Andy X configuration file, this file should be saved into config directory of Andy X Connect before running this service.

	{
		"NodeUrl": "https://localhost:9001",
		"Tenant": "{tenantName}",
		"Product": "{productName}",
		"Component": "{componentName}"
	}

## How to Engage, Contribute, and Give Feedback

Some of the best ways to contribute are to try things out, file issues, join in design conversations,
and make pull-requests.

## Reporting security issues and bugs

Security issues and bugs should be reported privately, via email, en.buildersoft@gmail.com. You should receive a response within 24 hours.

## Related projects

These are some other repos for related projects:

* [Andy X Dashboard](https://github.com/buildersoftdev/andyxdashboard) - Dashboard for Andy X Node
* [Andy X Terminal](https://github.com/buildersoftdev/andyxterminal) - Manage all resources of Andy X

## Deploying Andy X Connect with docker-compose

Andy X Connect can be easily deployed on a docker container using docker-compose, for more info click [here](https://hub.docker.com/r/buildersoftdev/andyx-mssql-adapter)

    version: '3.4'
    
    services:
        andyx-mssql-adapter:
        container_name: andyx-mssql-adapter
        image: buildersoftdev/andyx-mssql-adapter:1.0.0-preview
    
        volumes:
            - ./sqlconnection_config.json:/app/config/sqlconnection_config.json
            - ./xnode_config.json:/app/config/xnode_config.json

Network configuration using docker-compose is not needed if only Andy X Connect is deployed. Network should be configured if this adapter will be deployed together with Andy X and Andy X Storage.

Below is an example of deploying Andy X, Andy X Storage, Andy X Connect and Microsoft SQL Server, if you have problems deploying Andy X via docker-compose please click [here](https://hub.docker.com/r/buildersoftdev/andyx).

	version: '3.4'
	
	services:
		andyxstorage2:
		container_name: andyxstorage
		image: buildersoftdev/andyxstorage:2.0.1-preview
		ports:
			- 9002:443
		environment:
			- ASPNETCORE_ENVIRONMENT=Development
			- ASPNETCORE_URLS=https://+:443
			- ASPNETCORE_Kestrel__Certificates__Default__Password={password}
			- ASPNETCORE_Kestrel__Certificates__Default__Path=/https/{domain}_private_key.pfx
			- XNodes__0:ServiceUrl=andyxnode
			- XNodes__0:Subscription=1
			- XNodes__0:JwtToken=na
			- DataStorage:Name=andyxstorage
	
		volumes:
			- ~/.aspnet/https:/https:ro
			- ./data:/app/data
		networks:
			- local
	
	# ----------------------------------------------------------------------------------------------------
	
		andyx-mssql-adapter:
		container_name: andyx-mssql-adapter
		image: buildersoftdev/andyx-mssql-adapter:1.0.1-preview
		volumes:
            # -- In the same folder with docker-compose should be these two files, before running docker-compose. 
			- ./sqlconnection_config.json:/app/config/sqlconnection_config.json
			- ./xnode_config.json:/app/config/xnode_config.json
		networks:
			- local
	
	# ----------------------------------------------------------------------------------------------------
		
		andyx2:
		container_name: andyxnode
		image: buildersoftdev/andyx:2.0.1-preview
		ports:
			- 9001:443
		environment:
			- ASPNETCORE_ENVIRONMENT=Development
			- ASPNETCORE_URLS=https://+:443
			- ASPNETCORE_Kestrel__Certificates__Default__Password={password}
			- ASPNETCORE_Kestrel__Certificates__Default__Path=/https/{domain}_private_key.pfx
		volumes:
			- ~/.aspnet/https:/https:ro
		networks:
			- local
	
	# ----------------------------------------------------------------------------------------------------
			
		sql-server:
		image: mcr.microsoft.com/mssql/server
		hostname: sql-server
		container_name: sql-server
		ports:
			- "1433:1433"
		environment:
			- ACCEPT_EULA=Y
			- MSSQL_SA_PASSWORD=YourStrong!Passw0rd
			- MSSQL_PID=Express
		networks:
			- local
			
	# ----------------------------------------------------------------------------------------------------
	
	networks:
		local:
		driver: bridge

To run Andy X Connect with docker-compse you should execute 

    docker-compose up -d

## Code of conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.

For more information, see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

## Support
Let's do it together! You can support us by clicking on the link below!

[![alt text](https://img.buymeacoffee.com/api/?url=aHR0cHM6Ly9pbWcuYnV5bWVhY29mZmVlLmNvbS9hcGkvP3VybD1hSFIwY0hNNkx5OWpaRzR1WW5WNWJXVmhZMjltWm1WbExtTnZiUzkxY0d4dllXUnpMM0J5YjJacGJHVmZjR2xqZEhWeVpYTXZNakF5TVM4d09DOWxObVUwTkRWaU1UVXhPVGRqWm1JNFlXWTVZalV5TWpjek5qSXlaV05rTnk1d2JtYz0mc2l6ZT0zMDAmbmFtZT1BbmR5K1g=&creator=Andy+X&is_creating=free%20and%20open%20source%20Distributed%20Streaming%20Platform&design_code=1&design_color=%2379D6B5&slug=buildersoft)](https://www.buymeacoffee.com/buildersoft).
