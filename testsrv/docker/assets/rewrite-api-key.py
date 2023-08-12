import sqlalchemy
import traceback

try:
    api_key = "1111222233334444555566667777888899990000"

    db_uri = 'sqlite:////kallithea/config/kallithea.db?timeout=60'
    engine = sqlalchemy.engine.create_engine(db_uri)
    with engine.connect() as db:
        affect = db.execute(f"update users set api_key = '{api_key}' where username = 'admin'")
        if affect.rowcount <= 0:
            raise Exception('Failed to rewrite')

except Exception:
    print(traceback.format_exc())
    exit(1)
