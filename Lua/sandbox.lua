function()
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

