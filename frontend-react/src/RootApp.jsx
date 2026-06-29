import { useEffect, useState } from "react";
import App from "./App";
import MosslightHollowPage from "./MosslightHollowPage";

function getPath() {
  return window.location.pathname || "/";
}

export default function RootApp() {
  const [path, setPath] = useState(getPath);

  useEffect(() => {
    function handlePopState() {
      setPath(getPath());
    }

    window.addEventListener("popstate", handlePopState);
    return () => window.removeEventListener("popstate", handlePopState);
  }, []);

  if (path === "/game") {
    return <MosslightHollowPage />;
  }

  return <App />;
}