import sqlite3
from datetime import datetime

conn = sqlite3.connect('finuchet.db')
cursor = conn.cursor()

# Создаем таблицу investments
cursor.execute('''
CREATE TABLE IF NOT EXISTS investments (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    type TEXT NOT NULL,
    amount REAL DEFAULT 0,
    purchase_price REAL DEFAULT 0,
    current_price REAL DEFAULT 0,
    quantity REAL DEFAULT 0,
    purchase_date TIMESTAMP,
    currency TEXT DEFAULT 'RUB',
    notes TEXT DEFAULT '',
    user_id INTEGER,
    FOREIGN KEY (user_id) REFERENCES users (id)
)
''')

print("Таблица investments создана или уже существует")

conn.commit()
conn.close()
print("Готово!")