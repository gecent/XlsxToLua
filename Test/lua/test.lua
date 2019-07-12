----@classdef record_test
local record_test = {}


record_test.id = 0--ID
record_test.name = ""--名字
record_test.alias = ""--称号
record_test.level = 0--等级

local test = {
   _data = {
    [1] = {1,"_TEST_NAME_1_44","_TEST_ALIAS_1_44",44,},
    [2] = {2,"_TEST_NAME_2_33","_TEST_ALIAS_2_33",33,},
    [3] = {2,"_TEST_NAME_2_6","_TEST_ALIAS_2_6",6,},
    [4] = {2,"_TEST_NAME_2_20","_TEST_ALIAS_2_20",20,},
    [5] = {4,"_TEST_NAME_4_34","_TEST_ALIAS_4_34",34,},
    [6] = {22,"_TEST_NAME_22_5","_TEST_ALIAS_22_5",5,},
   }
}

local __index_id_level = {
    ["1_44"] = 1,
    ["2_33"] = 2,
    ["2_6"] = 3,
    ["2_20"] = 4,
    ["4_34"] = 5,
    ["22_5"] = 6,
}

local __key_map = {
    id = 1,
    name = 2,
    alias = 3,
    level = 4,
}

local m = {
    __index = function(t, k)
        if k == "toObject" then
            return function()
                local o = { }
                for key, v in pairs(__key_map) do
                    o[key] = t._raw[v]
                end
                return o
            end
        end

        assert(__key_map[k], "cannot find " ..k.. " in record_test ")
        return t._raw[__key_map[k]]
    end
}

function test.getLength()
    return #test._data
end

function test.hasKey(k)
    if __key_map[k] == nil then
        return false
    else
        return true
    end
end

---
--@return @class record_test
function test.indexOf(index)
    if index == nil then
        return nil
    end
    return setmetatable({_raw = test._data[index]}, m)
end

---
--@return @class record_test
function test.get(id, level)
    local k = id.. '_' .. level
    return test.indexOf(__index_id_level[ k ])     
end

function test.set(id, level, key, value)
    local record = test.get(id, level)
    if record then
        local keyIndex = __key_map[key]
        if keyIndex then
            record._raw[keyIndex] = value
        end
    end
end

function test.get_index_data()
    return __index_id_level 
end

function test.find(cb)
    for i=1, test.getLength(),1 do
        local rec = test.indexOf(i)
        if cb(rec)==true then
            return rec
        end
    end
    return nil
end

return test
