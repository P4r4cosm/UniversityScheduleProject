CREATE (:Lecture {id: 1, name: 'Введение в архитектуру ПО', requirments: true});

CREATE (:Group {id: 1, name: 'БСБО-01-22', startYear: '2022-09-01', endYear: '2026-06-30'});

CREATE (:Student {id: 1, fio: 'Сухов Антон Алексеевич', date_of_recipient: '2022-09-01'});


MATCH (s:Student {id: 1}), (l:Lecture {id: 1})
CREATE (s)-[:ATTENDED {visitTime: '2023-09-01T10:41:00'}]->(l);

MATCH (g:Group {id: 1}), (l:Lecture {id: 1})
CREATE (g)-[:HAS_LECTURE {
  startTime: '2023-02-10T10:40:00',
  endTime: '2025-02-10T12:10:00'
}]->(l);

MATCH (s:Student {id: 1}), (l:Lecture {id: 1})
CREATE (s)-[:ATTENDED {visitTime: '2023-09-01T10:41:00'}]->(l);