const API_BASE_URL = "http://localhost:5095";

new Vue({
  el: "#app",
  data: {
    username: sessionStorage.getItem("username") || "",
    usernameInput: "",
    newTodo: "",
    todos: [],
    showModal: false,
  },
  methods: {
    login() {
      this.username = this.usernameInput;
      sessionStorage.setItem("username", this.username);
      this.fetchTodos();
    },
    fetchTodos() {
      fetch(`${API_BASE_URL}/todo/${this.username}`)
        .then((response) => response.json())
        .then((data) => {
          this.todos = data;
        });
    },
    addTodo() {
      if (this.newTodo) {
        const newTodo = {
          TaskDescription: this.newTodo,
          Username: this.username,
          CreatedBy: new Date().toISOString(),
          Completed: false,
          CompletedOn: null,
        };
        fetch(`${API_BASE_URL}/todo`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(newTodo),
        })
          .then((response) => response.json())
          .then((data) => {
            this.todos.push(data);
            this.newTodo = "";
            this.showModal = false;
          });
      }
    },
    toggleComplete(id) {
      const todo = this.todos.find((todo) => todo.id === id);
      fetch(`${API_BASE_URL}/todo/completed/${id}`, {
        method: "POST",
      })
        .then((response) => response.json())
        .then((updatedTodo) => {
          todo.completed = updatedTodo.completed;
          todo.completedOn = updatedTodo.completedOn;
        });
    },
    deleteAllTasks() {
      fetch(`${API_BASE_URL}/todo/${this.username}`, {
        method: "DELETE",
      }).then(() => {
        this.todos = [];
      });
    },
  },
  created() {
    if (this.username) {
      this.fetchTodos();
    }
  },
});
