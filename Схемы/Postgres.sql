
--Универы
CREATE TABLE "universities" (
	"id" integer PRIMARY KEY,
	"name" text
);
--Институты
CREATE TABLE "institutes" (
  "id" integer PRIMARY KEY,
  "id_univer" integer,
  "name" text
);
ALTER TABLE "institutes" ADD FOREIGN KEY ("id_univer") REFERENCES "universities" ("id");

--Кафедры
CREATE TABLE "departments" (
  "id" integer PRIMARY KEY,
  "id_institutes" integer,
  "name" text
);
ALTER TABLE "departments" ADD FOREIGN KEY ("id_institutes") REFERENCES "institutes" ("id");

--Группы
CREATE TABLE "groups" (
  "id" integer PRIMARY KEY,
  "id_kafedra" integer,
  "name" text,
  "startYear" date,
  "endYear" date
);
ALTER TABLE "groups" ADD FOREIGN KEY ("id_kafedra") REFERENCES "departments" ("id");

--Студенты
CREATE TABLE "students" (
  "id" integer PRIMARY KEY,
  "id_group" integer,
  "fio" text,
  "date_of_recipient" date
);
ALTER TABLE "students" ADD FOREIGN KEY ("id_group") REFERENCES "groups" ("id");


--специальности
CREATE TABLE "specialties" (
  "id" integer PRIMARY KEY,
  "name" text,
  "code" text
);

--курсы
CREATE TABLE "courses" (
  "id" integer PRIMARY KEY,
  "id_kadefra" integer,
  "id_spec" integer,
  "name" text,
  "term" date
);
ALTER TABLE "courses" ALTER COLUMN "term" TYPE TEXT;
ALTER TABLE "courses" ADD FOREIGN KEY ("id_kadefra") REFERENCES "departments" ("id");
ALTER TABLE "courses" ADD FOREIGN KEY ("id_spec") REFERENCES "specialties" ("id");


--лекции
CREATE TABLE "lectures" (
  "id" integer PRIMARY KEY,
  "id_course" integer,
  "name" text,
  "requirments" boolean
);
ALTER TABLE "lectures" ADD FOREIGN KEY ("id_course") REFERENCES "courses" ("id");

--материалы
CREATE TABLE "materials" (
  "id" integer PRIMARY KEY,
  "id_lect" integer,
  "name" text
);
ALTER TABLE "materials" ADD FOREIGN KEY ("id_lect") REFERENCES "lectures" ("id");

--расписания
CREATE TABLE "schedule" (
  "id" integer PRIMARY KEY,
  "id_lect" integer,
  "id_group" integer,
  "startTime" timestamptz,
  "endTime" timestamptz
);
ALTER TABLE "schedule" ADD FOREIGN KEY ("id_lect") REFERENCES "lectures" ("id");
ALTER TABLE "schedule" ADD FOREIGN KEY ("id_group") REFERENCES "groups" ("id");

--посещения
CREATE TABLE "visits" (
  "id" integer PRIMARY KEY,
  "code_student" integer,
  "id_rasp" integer,
  "visitTime" timestamptz
);
ALTER TABLE "visits" ADD FOREIGN KEY ("code_student") REFERENCES "students" ("id");
ALTER TABLE "visits" ADD FOREIGN KEY ("id_rasp") REFERENCES "schedule" ("id");





--Вставка данных
-- Добавление университета РТУ МИРЭА
INSERT INTO universities (id, name) VALUES (1, 'РТУ МИРЭА');

-- Добавление института ИКБ
INSERT INTO institutes (id, id_univer, name) VALUES (1, 1, 'Институт ИКБ');

-- Добавление кафедры разработки ПО
INSERT INTO departments (id, id_institutes, name) VALUES (1, 1, 'Факультет разработки ПО');

-- Добавление группы (пример: ПО-201)
INSERT INTO groups (id, id_kafedra, name, "startYear", "endYear")
VALUES (1, 1, 'БСБО-01-22', '2022-09-01', '2026-06-30');

-- Добавление студента
INSERT INTO students (id, id_group, fio, date_of_recipient)
VALUES (1, 1, 'Сухов Антон Алексеевич', '2022-09-01');

-- Добавление специальности
INSERT INTO specialties (id, name, code)
VALUES (1, 'Информационные системы и технологии', '09.03.02');

-- Добавление курса
INSERT INTO courses (id, id_kadefra, id_spec, name, term)
VALUES (1, 1, 1, 'Проектирование архитектуры программного обеспечения', '2025-2026');

-- Добавление лекции
INSERT INTO lectures (id, id_course, name, requirments)
VALUES (1, 1, 'Введение в архитектуру ПО', true);

-- Добавление материалов к лекции
INSERT INTO materials (id, id_lect, name)
VALUES (1, 1, 'Лекция 1: Основные концепции');

-- Добавление расписания
INSERT INTO schedule (id, id_lect, id_group, "startTime", "endTime")
VALUES (1, 1, 1, '2023-02-10 10:40:00', '2025-02-10 12:10:00');

-- Добавление посещения
INSERT INTO visits (id, code_student, id_rasp, "visitTime")
VALUES (1, 1, 1, '2023-09-01 10:41:00');