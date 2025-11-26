import './App.css'
import Header from './components/header/Header.tsx'
import Sidebar from './components/sidebar/Sidebar.tsx'

function App() {
  return (
    <div className="app-container">
      <Sidebar />
      <div className="main-content">
        <Header />
        <div className="editor">
          <p>Editor Canvas here.</p>
        </div>
      </div>
    </div>
  )
}

export default App