sub OnInit()
	Dim pli as PluginInstance = GetFunctionPluginInstance("Omo")
	if pli <> NULL then
		println("Omo plugin found")
		Dim testData as Integer = pli.GetParameterInt("vis_con")
		Dim eventName as String  = pli.VizEventName
		println(eventName)
		pli.EventPool.registerAsListener(eventName, "OnParameterChanged")
	else 
		println("Omo plugin Not found")
	end if
end sub

Dim OmoField as Integer = 0
Dim sliderDirector as Director = Stage.FindDirector("ScoreUpdate")
Dim pli as PluginInstance = GetFunctionPluginInstance("Omo")
Dim currentOmoValue as Integer = 0
Dim animationStarted as Boolean = False
Dim startPolling as Boolean  = False
Dim waitingPeriod as Double = 5
sub OnExecPerField()
	
	Dim tempValue as Integer = pli.GetParameterInt("vis_con")

	If currentOmoValue <> tempValue And tempValue <> 0 Then
		Println("Omo Value Changed")
		currentOmoValue = tempValue
		animationStarted = True
	End If
	
	If animationStarted Then
		If sliderDirector.isAnimationRunning() Then
			Println("Animation is running")
			animationStarted = False
			startPolling = True
		End If
	End If
	
	If startPolling Then
		If Not sliderDirector.isAnimationRunning() Then
			Println("Animation Stopped")
			startPolling = False
			pli.SetParameterInt("vis_con", 0)
		End If
	End If
	
	if pli <> NULL then
		Dim testData as Integer = pli.GetParameterInt("vis_con")
		if testData <> OmoField then
			omoField = testData
			println(OmoField)
			if sliderDirector <> NULL then
				sliderDirector.StartAnimation()
			else
				println("Director not found")			
			end if
			
		end if
	else 
		println("Omo plugin Not found")
	end if
	
end sub



