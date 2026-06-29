import { useEffect, useRef, useState } from "react";
import { getStoryOpening, sendStoryCommand } from "./api";
import "./MosslightHollowPage.css";

export default function MosslightHollowPage() {
  const [messages, setMessages] = useState([]);
  const [draft, setDraft] = useState("");
  const [isSending, setIsSending] = useState(false);
  const [isLoadingStory, setIsLoadingStory] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");
  const messagesRef = useRef(null);

  useEffect(() => {
    let isActive = true;

    async function loadOpening() {
      try {
        const response = await getStoryOpening();

        if (!isActive) {
          return;
        }

        setMessages([
          {
            id: Date.now(),
            role: "game",
            sceneName: response.sceneName ?? null,
            sceneId: response.sceneId ?? null,
            text: response.reply,
          },
        ]);
        setErrorMessage("");
      } catch (error) {
        if (!isActive) {
          return;
        }

        setMessages([
          {
            id: Date.now(),
            role: "game",
            sceneName: null,
            sceneId: null,
            text: "Mosslight Hollow is hidden in fog right now. Try again in a moment.",
          },
        ]);
        setErrorMessage(error.message ?? "Unable to open the story right now.");
      } finally {
        if (isActive) {
          setIsLoadingStory(false);
        }
      }
    }

    loadOpening();

    return () => {
      isActive = false;
    };
  }, []);

  useEffect(() => {
    const messagesElement = messagesRef.current;

    if (!messagesElement) {
      return;
    }

    messagesElement.scrollTo({
      top: messagesElement.scrollHeight,
      behavior: "smooth",
    });
  }, [messages]);

  async function handleSubmit(event) {
    event.preventDefault();

    const trimmedDraft = draft.trim();
    if (!trimmedDraft || isSending || isLoadingStory) {
      return;
    }

    const playerMessage = {
      id: Date.now(),
      role: "player",
      sceneName: null,
      sceneId: null,
      text: trimmedDraft,
    };

    setDraft("");
    setIsSending(true);
    setErrorMessage("");
    setMessages((currentMessages) => [...currentMessages, playerMessage]);

    try {
      const response = await sendStoryCommand(trimmedDraft);

      setMessages((currentMessages) => [
        ...currentMessages,
        {
          id: Date.now() + 1,
          role: "game",
          sceneName: response.sceneName ?? null,
          sceneId: response.sceneId ?? null,
          text: response.reply,
        },
      ]);
    } catch (error) {
      setErrorMessage(error.message ?? "The story path is quiet right now.");
    } finally {
      setIsSending(false);
    }
  }

  const visibleMessages = messages.slice(-10);

  return (
    <main className="story-page">
      <header className="story-page__header">
        <a className="story-page__back-link" href="/">
          Back to Den
        </a>
        <div>
          <p className="story-page__eyebrow">Story Game Preview</p>
          <h1>Mosslight Hollow</h1>
        </div>
      </header>

      <section className="story-panel" aria-label="Story transcript">
        <div className="story-panel__messages" ref={messagesRef}>
          {visibleMessages.map((message) => (
            <article
              key={message.id}
              className={`story-bubble story-bubble--${message.role}`}
            >
              <div className="story-bubble__meta">
                <p className="story-bubble__label">
                  {message.role === "player" ? "You" : "Story"}
                </p>
                {message.sceneName ? (
                  <span className="story-bubble__scene-pill">
                    {message.sceneName}
                  </span>
                ) : null}
              </div>
              <p className="story-bubble__text">{message.text}</p>
            </article>
          ))}
        </div>

        <form className="story-panel__composer" onSubmit={handleSubmit}>
          <label className="story-panel__label" htmlFor="story-command">
            What do you do?
          </label>
          <div className="story-panel__input-row">
            <input
              id="story-command"
              name="story-command"
              type="text"
              value={draft}
              onChange={(event) => setDraft(event.target.value)}
              placeholder={isLoadingStory ? "Gathering the story..." : "Try: look, go east, answer 7, take lantern, or inventory"}
              autoComplete="off"
              disabled={isSending || isLoadingStory}
            />
            <button type="submit" disabled={isSending || isLoadingStory || !draft.trim()}>
              {isSending ? "Sending..." : "Send"}
            </button>
          </div>
          {errorMessage ? <p className="story-panel__error">{errorMessage}</p> : null}
        </form>
      </section>
    </main>
  );
}
