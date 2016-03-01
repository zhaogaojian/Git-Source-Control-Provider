$gitHubUsername = "$env.gitHubUsername" 
$gitHubRepository = "$env.gitRepo"
$tagName = "GSCP-Dev-v$env:majorVersion.$env.minorVersion"
$gitHubApiKey = "$env.access_token"

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
	Write-Host "Release not found" 
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
  $result = Invoke-RestMethod @deleteTagParams 