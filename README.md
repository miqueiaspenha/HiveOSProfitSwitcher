# HiveOSProfitSwitcher
This is a .NET Executable that can connect to your Hive OS account and profit switch your mining rigs based on WhatToMine calculations and using a configuration that you setup. 

Please note that there is a 5% buffer on current coin vs new profitability. This limits constant flipping of coins when they are within a couple of cents of each other. If the new top coin is 4% better in profitability, it will remain on the current coin until that new top one exceeds 5% of the one currently being mined.

Please also note that this exe doesn't loop. It's intended to be run under a cron job or task scheduler. You will need to configure this to run reoccuring based on your own environment requirements. I'll be putting out some videos soon on how to make this run on one of your existing hive os rigs.

If you would like to support me, please feel free to check out my youtube channel at: https://www.youtube.com/channel/UCrpTQG3FEs0p4IPMKSMT9Dw

- Config Options Needed
  - Hive OS API Key (You can get this by logging into your account, clicking your username at the top right, selecting the Sessions tab, and then creating a new API token)
  - Hive OS Farm ID (You can get this from your farm URL)
  - WhatToMine JSON (Go to WhatToMine, fill in your hashrates, hit calculate, then click JSON at the top, and copy the URL from the address bar
  - Worker Config section. This is where you specify the worker name, link to WhatToMine JSON, and what coins you want it to profit switch between.
  
#### Example Config
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<configSections>
		<section name="profitSwitching" type="HiveProfitSwitcher.ProfitSwitchingConfig, HiveProfitSwitcher" />
	</configSections>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2" />
    </startup>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.0.0.0" newVersion="5.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
	<appSettings>
		<add key="HiveOSApiKey" value="xxxxx"/>
		<add key="HiveFarmId" value="xxxxx"/>
	</appSettings>
	<profitSwitching>
		<workers>
			<add name="2GB_RIG" whatToMineEndpoint="https://whattomine.com/coins.json?factor%5Beth_hr%5D=0.0&amp;factor%5Beth_p%5D=0.0&amp;e4g=true&amp;factor%5Be4g_hr%5D=122.8&amp;factor%5Be4g_p%5D=370.0&amp;zh=true&amp;factor%5Bzh_hr%5D=96.0&amp;factor%5Bzh_p%5D=358.0&amp;cnh=true&amp;factor%5Bcnh_hr%5D=2500.0&amp;factor%5Bcnh_p%5D=285.0&amp;cng=true&amp;factor%5Bcng_hr%5D=2774.0&amp;factor%5Bcng_p%5D=365.0&amp;cnf=true&amp;factor%5Bcnf_hr%5D=7700.0&amp;factor%5Bcnf_p%5D=340.0&amp;factor%5Bcx_hr%5D=0.0&amp;factor%5Bcx_p%5D=0.0&amp;eqa=true&amp;factor%5Beqa_hr%5D=400.0&amp;factor%5Beqa_p%5D=315.0&amp;factor%5Bcc_hr%5D=999.0&amp;factor%5Bcc_p%5D=0.0&amp;factor%5Bcr29_hr%5D=999.0&amp;factor%5Bcr29_p%5D=0.0&amp;factor%5Bct31_hr%5D=1.4&amp;factor%5Bct31_p%5D=440.0&amp;factor%5Bct32_hr%5D=0.4&amp;factor%5Bct32_p%5D=440.0&amp;factor%5Beqb_hr%5D=58.0&amp;factor%5Beqb_p%5D=480.0&amp;factor%5Brmx_hr%5D=1560.0&amp;factor%5Brmx_p%5D=320.0&amp;factor%5Bns_hr%5D=2800.0&amp;factor%5Bns_p%5D=560.0&amp;factor%5Bal_hr%5D=222.0&amp;factor%5Bal_p%5D=440.0&amp;factor%5Bops_hr%5D=0.0&amp;factor%5Bops_p%5D=0.0&amp;factor%5Beqz_hr%5D=999.0&amp;factor%5Beqz_p%5D=440.0&amp;factor%5Bzlh_hr%5D=50.0&amp;factor%5Bzlh_p%5D=400.0&amp;kpw=true&amp;factor%5Bkpw_hr%5D=48.6&amp;factor%5Bkpw_p%5D=321.0&amp;factor%5Bppw_hr%5D=31.2&amp;factor%5Bppw_p%5D=520.0&amp;x25x=true&amp;factor%5Bx25x_hr%5D=2.68&amp;factor%5Bx25x_p%5D=191.0&amp;factor%5Bfpw_hr%5D=0.0&amp;factor%5Bfpw_p%5D=0.0&amp;vh=true&amp;factor%5Bvh_hr%5D=2.6&amp;factor%5Bvh_p%5D=391.0&amp;factor%5Bcost%5D=0.1&amp;factor%5Bcost_currency%5D=USD&amp;sort=Profitability24&amp;volume=0&amp;revenue=24h&amp;factor%5Bexchanges%5D%5B%5D=&amp;factor%5Bexchanges%5D%5B%5D=binance&amp;factor%5Bexchanges%5D%5B%5D=bitfinex&amp;factor%5Bexchanges%5D%5B%5D=bitforex&amp;factor%5Bexchanges%5D%5B%5D=bittrex&amp;factor%5Bexchanges%5D%5B%5D=coinex&amp;factor%5Bexchanges%5D%5B%5D=dove&amp;factor%5Bexchanges%5D%5B%5D=exmo&amp;factor%5Bexchanges%5D%5B%5D=gate&amp;factor%5Bexchanges%5D%5B%5D=graviex&amp;factor%5Bexchanges%5D%5B%5D=hitbtc&amp;factor%5Bexchanges%5D%5B%5D=hotbit&amp;factor%5Bexchanges%5D%5B%5D=ogre&amp;factor%5Bexchanges%5D%5B%5D=poloniex&amp;factor%5Bexchanges%5D%5B%5D=stex&amp;dataset=Main">
				<enabledCoins>
					<add coinTicker="VTC" flightSheetName="2GB_RIG_VTC" />
					<add coinTicker="UBQ" flightSheetName="2GB_RIG_UBQ" />
					<add coinTicker="NEOX" flightSheetName="2GB_RIG_NEOX" />
					<add coinTicker="SIN" flightSheetName="2GB_RIG_SIN" />
					<add coinTicker="MSR" flightSheetName="2GB_RIG_MSR" />
					<add coinTicker="BTG" flightSheetName="2GB_RIG_BTG" />
					<add coinTicker="XHV" flightSheetName="2GB_RIG_XHV" />
					<add coinTicker="AION" flightSheetName="2GB_RIG_AION" />
					<add coinTicker="RYO" flightSheetName="2GB_RIG_RYO" />
				</enabledCoins>
			</add>
		</workers>
	</profitSwitching>
</configuration>
```
