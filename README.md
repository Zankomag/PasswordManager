# Password Manager Bot
Asp NET Core bot that helps you to manage your accounts.

!Working build: https://github.com/Zankomag/PasswordManager/tree/3ae4161bd7a14c2ed2a3c7ac546bc9cfb8cbfeb4!

###How to create database using EF Core
Make sure you have ef core dotnet CLI tools installed. If not, run:
```
dotnet tool install --global dotnet-ef
```
Make sure connection string is set in appsettings.Environment.json. After that run:
```
cd src/PasswordManager.Infrastructure
dotnet ef --startup-project ../PasswordManager.Web/ migrations add Initial
dotnet ef --startup-project ../PasswordManager.Web/ database update
```


## How to deploy on Linux server
There must be installed dotnet core, nginx and SSL certificate on your web-server to use webhook.
Do not forget to update SSL certificate in time!

In Program.cs you can change port to whatever you want.
        
Then you have to build bot. Stay in folder that contains .csproj file and write:
```
sudo dotnet publish -o /path/to/output/directory
```
Next you need to configure Nginx as a reverse proxy to forward requests to ASP.NET Core app: modify target file at /etc/nginx/sites-available/ (basic server configuration must exist)

```
location /api/bots/BOT_TOKEN {
        proxy_pass         http://localhost:TARGET_PORT;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
```
Restart Nginx:
```
sudo service nginx restart
```
### How to run bot
There are two ways to run bot:
- Open folder what contains built bot (location of UPwdBot.dll) (you specified this folder in dotnet publish).
Then type the following:
```
sudo nohup dotnet UPwdBot.dll &
```
To stop the bot using this way you have to kill bot process.

- Run bot as service
```
sudo nano /etc/systemd/system/PasswordManagerBot.service
```
Then, inside that file paste its configuration:
```
[Unit] 
Description=Password Manager Bot

[Service] 
WorkingDirectory=/path/to/publish/output/folder 
ExecStart=/usr/bin/dotnet /path/to/publish/output/folder/UPwdBot.dll 
Restart=no
RestartSec=10
SyslogIdentifier=offershare-web-app
Environment=ASPNETCORE_ENVIRONMENT=Production 

[Install] 
WantedBy=multi-user.target
```
Restart field can be set to "always" instead of "no" if you wish app to be restarted after crash

To make daemon service start on server boot:
```
sudo systemctl enable PasswordManagerBot
```

You can check if it's enabled via:
```
sudo service PasswordManagerBot status
```
It must say "Loaded: loaded (/etc/systemd/system/PasswordManagerBot.service; enabled;"

After service configured:
```
sudo service PasswordManagerBot start
```
To stop bot:
```
sudo service PasswordManagerBot stop
```
