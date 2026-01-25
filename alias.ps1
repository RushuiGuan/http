if ($IsMacOS) {
	set-alias -name sample -Value $env:InstallDirectory\Sample.CommandLine\Sample.CommandLine
	set-alias -name test -Value $env:InstallDirectory\Sample.WebApi\Sample.WebApi
}else {
	set-alias -name sample -Value $env:InstallDirectory\Sample.CommandLine\Sample.CommandLine.exe
	set-alias -name api -Value $env:InstallDirectory\Sample.WebApi\Sample.WebApi.exe
}


