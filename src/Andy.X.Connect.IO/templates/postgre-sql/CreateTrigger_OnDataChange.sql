CREATE TRIGGER "andyx_{table_name}_OnDataChange"
  AFTER INSERT OR DELETE OR UPDATE 
  ON {database_name}.{table_name}
  FOR EACH ROW
  EXECUTE PROCEDURE {database_name}."andyx_{table_name}_NotifyOnDataChange"();
