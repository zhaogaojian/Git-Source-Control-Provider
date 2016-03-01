param (
    [string]$gitHubUsername = $null,
    [string]$gitHubRepository = $null,
    [string]$tagName = $null,
    [string]$gitHubApiKey = $null
)


if ($gitHubUsername.Length -eq 0) {
    Write-Host "Parameter -gitHubUsername was not provided, and is required."
    return
}

if ($gitHubRepository.Length -eq 0) {
    Write-Host "Parameter -gitHubRepository was not provided, and is required."
    return
}

if ($tagName.Length -eq 0) {
    Write-Host "Parameter -tagName was not provided, and is required."
    return
}
if ($gitHubApiKey.Length -eq 0) {
    Write-Host "Parameter -gitHubApiKey was not provided, and is required."
    return
}

 $getReleaseParams = @{
   Uri = "https://api.github.com/repos/$gitHubUsername/$gitHubRepository/releases/tags/$tagName";
   Method = 'GET';
   Headers = @{
     Authorization = 'token ' + $gitHubApiKey
   }
 }

$relId;

Try
{
	$release = Invoke-RestMethod @getReleaseParams 
	$relId = $release.id
}
Catch
{
	Write-Host "Release not found" 
  Exit;
}

if (!$relId)
{
	Write-Host "Release ID not found" 
	Exit;
}



 $deleteReleaseParams = @{
   Uri = "https://api.github.com/repos/$gitHubUsername/$gitHubRepository/releases/$relId";
   Method = 'DELETE';
   Headers = @{
      Authorization = 'token ' + $gitHubApiKey
   }
 }

  $deleteTagParams = @{
   Uri = "https://api.github.com/repos/$gitHubUsername/$gitHubRepository/git/refs/tags/$tagName";
   Method = 'DELETE';
   Headers = @{
     Authorization = 'token ' + $gitHubApiKey
   }
      ContentType = 'application/json';
   Body = ""
 }

  $result = Invoke-RestMethod @deleteReleaseParams
  Write-Host "Release Deleted"    
  $result = Invoke-RestMethod @deleteTagParams 
  Write-Host "Tag Deleted"    