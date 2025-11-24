import './App.css'

function App() {
  return (
    <div className="app-container">
      <header className="header">
        <h1>Behavior Tree Editor</h1>
      </header>

      <div className="layout">
        <aside className="sidebar">
          <p>Nodes</p>
          <ul>
            <li>Selector</li>
            <li>Sequence</li>
            <li>Condition</li>
            <li>Action</li>
          </ul>
        </aside>

        <main className="editor">
          <p>Editor Canvas</p>
        </main>
      </div>
    </div>
  )
}

export default App
