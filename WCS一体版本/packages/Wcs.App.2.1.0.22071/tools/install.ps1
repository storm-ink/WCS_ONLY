param($installPath, $toolsPath, $package, $project)
$itemArray="系统配置\堆垛机\alarms.xml","系统配置\堆垛机\C001.xml","系统配置\基本配置\routes.xml","系统配置\基本配置\settings.xml","系统配置\基本配置\startups.xml","系统配置\基本配置\taskEventHandlers.xml","系统配置\输送线\db1.xml","系统配置\输送线\db2.xml","系统配置\输送线\输送线位置.xml","Wcs.App.exe","Wcs.App.exe.config"

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