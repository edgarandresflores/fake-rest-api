var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME")))
{
    app.UseHttpsRedirection();
}

var students = new List<Student>
{
    new(1, "Ana", "Lopez", "ana.lopez@escuela.edu", DateOnly.FromDateTime(DateTime.Today.AddYears(-1))),
    new(2, "Bruno", "Diaz", "bruno.diaz@escuela.edu", DateOnly.FromDateTime(DateTime.Today.AddMonths(-6)))
};
var teachers = new List<Teacher>
{
    new(1, "Carla", "Rojas", "carla.rojas@escuela.edu"),
    new(2, "Diego", "Perez", "diego.perez@escuela.edu")
};
var courses = new List<Course>
{
    new(1, "Matematica Basica", 4, 1),
    new(2, "Lengua y Literatura", 3, 2)
};
var enrollments = new List<Enrollment>
{
    new(1, 1, 1, "A"),
    new(2, 2, 2, "B")
};

var nextStudentId = students.Max(s => s.Id) + 1;
var nextTeacherId = teachers.Max(t => t.Id) + 1;
var nextCourseId = courses.Max(c => c.Id) + 1;
var nextEnrollmentId = enrollments.Max(e => e.Id) + 1;

var studentsGroup = app.MapGroup("/api/students").WithTags("Students");
studentsGroup.MapGet("/", () => Results.Ok(students))
    .WithName("GetStudents");
studentsGroup.MapGet("/{id:int}", (int id) =>
    students.FirstOrDefault(s => s.Id == id) is { } student
        ? Results.Ok(student)
        : Results.NotFound())
    .WithName("GetStudentById");

studentsGroup.MapPost("/", (StudentCreate request) =>
{
    var student = new Student(nextStudentId++, request.FirstName, request.LastName, request.Email, request.EnrollmentDate);
    students.Add(student);
    return Results.Created($"/api/students/{student.Id}", student);
})
    .WithName("CreateStudent");


    
studentsGroup.MapPut("/{id:int}", (int id, StudentUpdate request) =>
{
    var index = students.FindIndex(s => s.Id == id);
    if (index < 0)
    {
        return Results.NotFound();
    }

    var current = students[index];
    var updated = current with
    {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        EnrollmentDate = request.EnrollmentDate
    };
    students[index] = updated;
    return Results.Ok(updated);
})
    .WithName("UpdateStudent");
studentsGroup.MapDelete("/{id:int}", (int id) =>
{
    var index = students.FindIndex(s => s.Id == id);
    if (index < 0)
    {
        return Results.NotFound();
    }

    students.RemoveAt(index);
    enrollments.RemoveAll(e => e.StudentId == id);
    return Results.NoContent();
})
    .WithName("DeleteStudent");

var teachersGroup = app.MapGroup("/api/teachers").WithTags("Teachers");
teachersGroup.MapGet("/", () => Results.Ok(teachers))
    .WithName("GetTeachers");
teachersGroup.MapGet("/{id:int}", (int id) =>
    teachers.FirstOrDefault(t => t.Id == id) is { } teacher
        ? Results.Ok(teacher)
        : Results.NotFound())
    .WithName("GetTeacherById");
teachersGroup.MapPost("/", (TeacherCreate request) =>
{
    var teacher = new Teacher(nextTeacherId++, request.FirstName, request.LastName, request.Email);
    teachers.Add(teacher);
    return Results.Created($"/api/teachers/{teacher.Id}", teacher);
})
    .WithName("CreateTeacher");
teachersGroup.MapPut("/{id:int}", (int id, TeacherUpdate request) =>
{
    var index = teachers.FindIndex(t => t.Id == id);
    if (index < 0)
    {
        return Results.NotFound();
    }

    var current = teachers[index];
    var updated = current with
    {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email
    };
    teachers[index] = updated;
    return Results.Ok(updated);
})
    .WithName("UpdateTeacher");
teachersGroup.MapDelete("/{id:int}", (int id) =>
{
    var index = teachers.FindIndex(t => t.Id == id);
    if (index < 0)
    {
        return Results.NotFound();
    }

    teachers.RemoveAt(index);
    courses.RemoveAll(c => c.TeacherId == id);
    enrollments.RemoveAll(e => courses.All(c => c.Id != e.CourseId));
    return Results.NoContent();
})
    .WithName("DeleteTeacher");

var coursesGroup = app.MapGroup("/api/courses").WithTags("Courses");
coursesGroup.MapGet("/", () => Results.Ok(courses))
    .WithName("GetCourses");
coursesGroup.MapGet("/{id:int}", (int id) =>
    courses.FirstOrDefault(c => c.Id == id) is { } course
        ? Results.Ok(course)
        : Results.NotFound())
    .WithName("GetCourseById");
coursesGroup.MapPost("/", (CourseCreate request) =>
{
    if (teachers.All(t => t.Id != request.TeacherId))
    {
        return Results.BadRequest(new { message = "TeacherId does not exist." });
    }

    var course = new Course(nextCourseId++, request.Title, request.Credits, request.TeacherId);
    courses.Add(course);
    return Results.Created($"/api/courses/{course.Id}", course);
})
    .WithName("CreateCourse");
coursesGroup.MapPut("/{id:int}", (int id, CourseUpdate request) =>
{
    var index = courses.FindIndex(c => c.Id == id);
    if (index < 0)
    {
        return Results.NotFound();
    }

    if (teachers.All(t => t.Id != request.TeacherId))
    {
        return Results.BadRequest(new { message = "TeacherId does not exist." });
    }

    var current = courses[index];
    var updated = current with
    {
        Title = request.Title,
        Credits = request.Credits,
        TeacherId = request.TeacherId
    };
    courses[index] = updated;
    return Results.Ok(updated);
})
    .WithName("UpdateCourse");
coursesGroup.MapDelete("/{id:int}", (int id) =>
{
    var index = courses.FindIndex(c => c.Id == id);
    if (index < 0)
    {
        return Results.NotFound();
    }

    courses.RemoveAt(index);
    enrollments.RemoveAll(e => e.CourseId == id);
    return Results.NoContent();
})
    .WithName("DeleteCourse");

var enrollmentsGroup = app.MapGroup("/api/enrollments").WithTags("Enrollments");
enrollmentsGroup.MapGet("/", () => Results.Ok(enrollments))
    .WithName("GetEnrollments");
enrollmentsGroup.MapGet("/{id:int}", (int id) =>
    enrollments.FirstOrDefault(e => e.Id == id) is { } enrollment
        ? Results.Ok(enrollment)
        : Results.NotFound())
    .WithName("GetEnrollmentById");
enrollmentsGroup.MapPost("/", (EnrollmentCreate request) =>
{
    if (students.All(s => s.Id != request.StudentId))
    {
        return Results.BadRequest(new { message = "StudentId does not exist." });
    }
    if (courses.All(c => c.Id != request.CourseId))
    {
        return Results.BadRequest(new { message = "CourseId does not exist." });
    }

    var enrollment = new Enrollment(nextEnrollmentId++, request.StudentId, request.CourseId, request.Grade);
    enrollments.Add(enrollment);
    return Results.Created($"/api/enrollments/{enrollment.Id}", enrollment);
})
    .WithName("CreateEnrollment");
enrollmentsGroup.MapPut("/{id:int}", (int id, EnrollmentUpdate request) =>
{
    var index = enrollments.FindIndex(e => e.Id == id);
    if (index < 0)
    {
        return Results.NotFound();
    }

    if (students.All(s => s.Id != request.StudentId))
    {
        return Results.BadRequest(new { message = "StudentId does not exist." });
    }
    if (courses.All(c => c.Id != request.CourseId))
    {
        return Results.BadRequest(new { message = "CourseId does not exist." });
    }

    var current = enrollments[index];
    var updated = current with
    {
        StudentId = request.StudentId,
        CourseId = request.CourseId,
        Grade = request.Grade
    };
    enrollments[index] = updated;
    return Results.Ok(updated);
})
    .WithName("UpdateEnrollment");
enrollmentsGroup.MapDelete("/{id:int}", (int id) =>
{
    var index = enrollments.FindIndex(e => e.Id == id);
    if (index < 0)
    {
        return Results.NotFound();
    }

    enrollments.RemoveAt(index);
    return Results.NoContent();
})
    .WithName("DeleteEnrollment");


app.Run();

record Student(int Id, string FirstName, string LastName, string Email, DateOnly EnrollmentDate);
record Teacher(int Id, string FirstName, string LastName, string Email);
record Course(int Id, string Title, int Credits, int TeacherId);
record Enrollment(int Id, int StudentId, int CourseId, string Grade);

record StudentCreate(string FirstName, string LastName, string Email, DateOnly EnrollmentDate);
record StudentUpdate(string FirstName, string LastName, string Email, DateOnly EnrollmentDate);
record TeacherCreate(string FirstName, string LastName, string Email);
record TeacherUpdate(string FirstName, string LastName, string Email);
record CourseCreate(string Title, int Credits, int TeacherId);
record CourseUpdate(string Title, int Credits, int TeacherId);
record EnrollmentCreate(int StudentId, int CourseId, string Grade);
record EnrollmentUpdate(int StudentId, int CourseId, string Grade);
