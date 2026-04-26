import sqlite3

conn = sqlite3.connect('finuchet.db')
cursor = conn.cursor()

try:
    cursor.execute('ALTER TABLE users ADD COLUMN email TEXT')
    print("Колонка email добавлена")
except:
    print("Колонка email уже существует")

try:
    cursor.execute('ALTER TABLE users ADD COLUMN security_question TEXT DEFAULT "Какую кличку у вас было в детстве?"')
    print("Колонка security_question добавлена")
except:
    print("Колонка security_question уже существует")

try:
    cursor.execute('ALTER TABLE users ADD COLUMN security_answer TEXT DEFAULT ""')
    print("Колонка security_answer добавлена")
except:
    print("Колонка security_answer уже существует")

conn.commit()
conn.close()
print("Готово!")