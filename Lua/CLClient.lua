function TestString()
	return "Test Approved!";
end

function GetCityInfo()
	local listItems = {};
	local i = 0;
	local z = 1;
	for i = 0, PlayerManager.GetWasEverAliveCount()-1, 1 do
		local pPlayer = Players[i];
		local pPlayerConfig = PlayerConfigurations[i];

		if pPlayer:WasEverAlive()  then
			local strPlayer = pPlayerConfig:GetCivilizationShortDescription();
			strPlayer = string.gsub(strPlayer, "LOC_CIVILIZATION_", "");
			if (#strPlayer == 0) then
				strPlayer = "Player " .. tostring(i);
			end
			
			local pCities = pPlayer:GetCities();
			local pCity;
			for ii, pCity in pCities:Members() do
				local population = pCity:GetPopulation();
				local housing = pCity:GetGrowth():GetHousing();
				local foodSurplus = pCity:GetGrowth():GetFoodSurplus();
				local buildNow = pCity:GetBuildQueue():CurrentlyBuilding();
                
				local cityName = string.gsub(pCity:GetName(), "LOC_CITY_NAME_", "");

				local str = pPlayer:GetID() .. ";" .. pCity:GetID() .. ";" .. cityName .. ";" .. pCity:GetX() .. ";" .. pCity:GetY() .. ";" .. population .. ";" .. housing .. ";" .. foodSurplus ..";" .. buildNow;
				listItems[z] = str;
				z = z + 1;			
			end		
		end
	end
  return listItems;
end

function GetUnitInfo()
	local listItems = {};
	local i = 0;
	local z = 1;
	for i = 0, PlayerManager.GetWasEverAliveCount()-1, 1 do
		local pPlayer = Players[i];
		local pPlayerConfig = PlayerConfigurations[i];

		if pPlayer:WasEverAlive()  then
			local strPlayer = pPlayerConfig:GetCivilizationShortDescription();
			if (#strPlayer == 0) then
				strPlayer = "Player " .. tostring(i);
			end
			
			local pUnits = pPlayer:GetUnits();
			local pUnit;
			for ii, pUnit in pUnits:Members() do
				local typeName = string.gsub(GameInfo.Units[pUnit:GetType()].UnitType, "UNIT_", "");	
				
				local promotionText = "";
				local unitExperience = pUnit:GetExperience();
				for promotion in GameInfo.UnitPromotions() do
					if(unitExperience:HasPromotion(promotion.Index) ) then
						promotionText = promotionText .. "," .. promotion.Index.ToString();
					end
				end		

				local fortifyTurns = (pUnit:GetFortifyTurns() > 0) and 1 or 0;

				local str = pPlayer:GetID() .. ";" .. pUnit:GetID() .. ";" .. typeName .. ";" .. pUnit:GetX() .. ";" .. pUnit:GetY() .. ";" .. pUnit:GetDamage() .. ";" .. fortifyTurns .. ";" .. promotionText;
				listItems[z] = str;
				z = z + 1;			
			end	
		end
	end
  return listItems;
end

function GetMapInfo()
   	local listItems = {};
   	local iW, iH = Map.GetGridSize();
   	local z = 1;
   	for x = 0, iW - 1 do
	    for y = 0, iH - 1 do
			local plot = Map.GetPlot(x, y);
			local terrain = plot:GetTerrainType();
			local feature = plot:GetFeatureType();
			local resource = plot:GetResourceType() .. "," .. plot:GetResourceCount();
			local improvement = plot:GetImprovementType() .. "," .. plot:GetRouteType();
			local pillaged = (plot:IsImprovementPillaged() and 1 or 0) .. "," .. (plot:IsRoutePillaged() and 1 or 0);
			local yield = "";
			for row in GameInfo.Yields() do
			    yield = yield .. "," .. plot:GetYield(row.Index);
		    end

			listItems[z] = terrain .. ";" .. feature .. ";" .. resource .. ";" .. improvement .. ";" .. pillaged .. ";" .. yield;
			z = z + 1;			
		end
	end
	return listItems;
end

function GetTechCivicInfo()
	local listItems = {};
    local z = 1;
	for iPlayer = 0, PlayerManager.GetWasEverAliveCount()-1 do
		local player = Players[iPlayer];
		if player:WasEverAlive() then
			local obtained = "";
   			for tech in GameInfo.Technologies() do
				local playerTechs = player:GetTechs();
				obtained = obtained .. "," .. (playerTechs:HasTech(tech.Index) and 1 or 0) * 2 + (playerTechs:HasBoostBeenTriggered(tech.Index) and 1 or 0);
			end
			listItems[z] = obtained;
			z=z+1;
	    end
	end
	
	for iPlayer = 0, PlayerManager.GetWasEverAliveCount()-1 do
		local player = Players[iPlayer];
		if player:WasEverAlive() then
			local obtained = "";
   			for civic in GameInfo.Civics() do
		    local playerCivics = player:GetCulture();
				obtained = obtained .. "," .. (playerCivics:HasCivic(civic.Index) and 1 or 0) * 2 + (playerCivics:HasBoostBeenTriggered(civic.Index) and 1 or 0);
			end
			listItems[z] = obtained;
			z=z+1;
	    end
	end
	return listItems;
end

function GetTotalYieldInfo()
   	local listItems = {};
    local z = 1;
   	
	for iPlayer = 0, PlayerManager.GetWasEverAliveCount()-1 do
		local player = Players[iPlayer];
		local science = player:GetTechs():GetScienceYield();
		local culture = player:GetCulture():GetCultureYield();
		local gold = player:GetTreasury():GetGoldYield(); 
		local faith = player:GetReligion():GetFaithYield();
		print(player:GetStats():GetTourism());
		local tourism = player:GetStats():GetTourism();
		local military = player:GetStats():GetMilitaryStrength();
		listItems[z] = science .. "," .. culture .. "," .. gold .. "," .. faith .. "," .. tourism .. "," .. military;
		z = z + 1;
	end
	return listItems;
end

function GetReligionInfo()
   	local listItems = {};
    local obtained = "";
    local z = 1;
   	
	local pAllReligions = Game.GetReligion():GetReligions();

	for i, religionInfo in ipairs(pAllReligions) do
		if(m_pGameReligion:HasBeenFounded(religionInfo.Religion)) then
		    local religionData = GameInfo.Religions[religionInfo.Religion];
		    local type = religionData.ReligionType;
		    local founder = religionInfo.Founder;
			local name = Game.GetReligion():GetName(religionInfo.Religion);
			local beliefs = "";
    		for j, beliefIndex in ipairs(religion.Beliefs) do
    		    beliefs = beliefs .. "," .. beliefIndex;
    		end
    		listItem[z] = type .. "," .. founder .. "," .. name .. "," .. beliefs;
    		z = z + 1;
		end
    end
	return listItems;
end

