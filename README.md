# Password Manager Bot
Asp NET Core bot that helps you to manage your accounts.

## Important!
Current version of bot does not include encryption modules. Encryption system will be updated in future versions.

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

After service configured type:
```
sudo systemctl PasswordManagerBot.service
sudo service PasswordManagerBot start
```
To stop bot type:
```
sudo service PasswordManagerBot stop
```
