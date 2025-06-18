import express from 'express'
const app = express()
const port = 8081

process.on('unhandledRejection', err => {
  console.error('Unhandled promise rejection:', err)
  process.exit(1)
})

process.on('unhandledRejection', err => {
  console.error('Unhandled promise rejection:', err);
  process.exit(1);
});

// Our "database"
const users = {
  1: { id: 1, name: 'Leanne Graham', username: 'Bret', email: 'Sincere@april.biz' }
}

app.get('/users/:id', (req, res) => {
  const user = users[req.params.id]
  res.setHeader('Content-Type', 'application/json; charset=utf-8')
  if (user) {
    res.json(user)
  } else {
    res.status(404).send()
  }
});

// Start the server and export it for the test
const server = app
  .listen(port, () => {

    console.log(`Provider API listening on http://localhost:${port}`)
  })
  .on('error', err => {
    console.error('Failed to start server:', err)
    process.exit(1)
  })
export { server }
