function Check-ExitCode() {
	$exitCode = $LASTEXITCODE
	if ($exitCode -ne 0) {
		echo "Non-zero ($exitCode) exit code. Exiting..."
		exit $exitCode
	}
}

function Get-UsedTargetFramework() {
	return 'net7.0'
}