import { useEffect, useRef, useState } from "react";
import { sendStoryCommand } from "./api";
import "./MosslightHollow.css";

const INITIAL_MESSAGES = [
  {
    id: 1,
    role: "game",
    text: "Welcome to Mosslight Hollow. Type a thought, a question, or a simple action to begin.",
  },
];

export default function MosslightHollowPage() {
  const [messages, setMessages] = useState(INITIAL_MESSAGES);
  const [draft, setDraft] = useState("");
  const [isSending, setIsSending] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const messagesRef = useRef(null);

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
    if (!trimmedDraft || isSending) {
      return;
    }

    const playerMessage = {
      id: Date.now(),
      role: "player",
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
              <p className="story-bubble__label">
                {message.role === "player" ? "You" : "Story"}
              </p>
              <p>{message.text}</p>
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
              placeholder="Try: look around, open the gate, or ask a question"
              autoComplete="off"
              disabled={isSending}
            />
            <button type="submit" disabled={isSending || !draft.trim()}>
              {isSending ? "Sending..." : "Send"}
            </button>
          </div>
          {errorMessage ? <p className="story-panel__error">{errorMessage}</p> : null}
        </form>
      </section>
    </main>
  );
}
