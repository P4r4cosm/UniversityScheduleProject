db.universities.insertOne({
    _id: 1,
    name: "РТУ МИРЭА",
    institutes: [
      {
        _id: 1,
        name: "Институт ИКБ",
        departments: [
          {
            _id: 1,
            name: "Факультет разработки ПО"
          }
        ]
      }
    ]
  });
  
  db.universities.find({"institutes.departments.name":"Факультет разработки ПО"})