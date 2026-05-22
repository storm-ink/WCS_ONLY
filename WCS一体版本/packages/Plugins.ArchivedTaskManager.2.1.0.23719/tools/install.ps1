param($installPath, $toolsPath, $package, $project)
$itemArray="Plugins.ArchivedTaskManager.dll","Plugins.ArchivedTaskManager.xml","Plugins.ArchivedTaskManager.pdb"

foreach($itemName in $itemArray)
{
	$arr=$itemName-Split "\\"
	$projectItems=$null
	foreach($name in $arr)
	{
		if($projectItems -eq $null){
			$file=$project.ProjectItems.Item($name)
		}else{
			$file=$projectItems.Item($name)
		}
		
		if($file.ProjectItems -eq $null -or $file.ProjectItems.Count -eq 0){
		
			$file.Properties.Item("BuildAction").Value = 0
			$file.Properties.Item("CopyToOutputDirectory").Value =2

			$projectItems=$null
		}else{
			$projectItems = $file.ProjectItems
		}
	}
}


$project.Object.References | where { $_.Name -eq 'Plugins.ArchivedTaskManager' } | foreach { $_.Remove() }