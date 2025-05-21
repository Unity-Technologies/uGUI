# Text Link

You can use `<link="ID">my link</link>` to add link metadata to a text segment. The link ID should be unique to allow you to retrieve its ID and link text content when the user interacts with your text.

You do not have to give each link a unique ID. You can reuse IDs when it makes sense, for example when linking to the same data multiple times. The `linkInfo` array contains each ID only once.

While this link enables user interaction, it does not change the appearance of the linked text. You have to use other tags for that.
