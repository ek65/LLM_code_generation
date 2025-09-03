from openai import OpenAI

client = OpenAI(api_key='sk-proj-nuu4KEPE5cHjx9N9Xd0eT3BlbkFJDldSETFpRibSS2RmiCC2')


def queryLLM(system_prompt, user_prompt, temperature=0, model="gpt-4o", json = False):
    """
    
    """

    chat = client.chat.completions.create(
        model=model,
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt}
        ],
        response_format={'type': 'json_object' if json else 'text'},
        temperature=temperature
    )
    return chat.choices[0].message.content



