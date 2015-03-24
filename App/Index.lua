local name 

if querystring['name'] == null then 
    name = 'world' 
else  
    name = querystring['name'] 
end

return "Hello " .. name
