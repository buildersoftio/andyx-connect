CREATE OR REPLACE FUNCTION {database_name}."andyx_{table_name}_NotifyOnDataChange"()
  RETURNS trigger
  LANGUAGE 'plpgsql'
AS $BODY$ 
DECLARE 
  data JSON;
  notification JSON;
BEGIN

  IF (TG_OP = 'DELETE') THEN
    data = row_to_json(OLD);
  ELSE
    data = row_to_json(NEW);
  END IF;
  notification = json_build_object(
            'table',TG_TABLE_NAME,
            'action', TG_OP,
            'data', data);     
    PERFORM pg_notify('andyx{table_name}datachange', notification::TEXT);
  RETURN NEW;
END
$BODY$;