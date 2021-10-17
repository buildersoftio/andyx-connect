-- Buildersoft ANDY X Connect
-- Code Generator
-- SQL Command - This is the first step

CREATE FUNCTION public."AndyX_{table_name}_NotifyOnDataChange"()
  RETURNS trigger
  LANGUAGE 'plpgsql'
AS $BODY$ 
DECLARE 
  data JSON;
  notification JSON;
BEGIN
  -- if we delete, then pass the old data
  -- if we insert or update, pass the new data
  IF (TG_OP = 'DELETE') THEN
    data = row_to_json(OLD);
  ELSE
    data = row_to_json(NEW);
  END IF;

  -- create json payload
  -- note that here can be done projection 
  notification = json_build_object(
            'table',TG_TABLE_NAME,
            'action', TG_OP, -- can have value of INSERT, UPDATE, DELETE
            'data', data);  
            
    -- note that channel name MUST be lowercase, otherwise pg_notify() won't work
    PERFORM pg_notify('andyx_{table_name}_datachange', notification::TEXT);
  RETURN NEW;
END
$BODY$;