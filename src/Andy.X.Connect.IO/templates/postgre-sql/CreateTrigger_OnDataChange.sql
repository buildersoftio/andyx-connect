-- Buildersoft
-- -- Andy X Conenct
-- -- # Template to create trigger onDataChanged for a table

CREATE TRIGGER "AndyX_{table_name}_OnDataChange"
  AFTER INSERT OR DELETE OR UPDATE 
  ON {database_name}.{table_name}
  FOR EACH ROW
  EXECUTE PROCEDURE public."AndyX_{table_name}_NotifyOnDataChange"();
