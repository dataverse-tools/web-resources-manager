wrm.exe init --config test.config.json --prefix test --root ..\WebResources --connection "Url=https://org.crm4.dynamics.com;AuthType=OAuth;Integrated Security=true;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto" 
wrm.exe push --config test.config.json --overwrite --connection "Url=https://org.crm4.dynamics.com;AuthType=OAuth;Integrated Security=true;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto" 
wrm.exe delete --config test.config.json --connection "Url=https://org.crm4.dynamics.com;AuthType=OAuth;Integrated Security=true;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto"
