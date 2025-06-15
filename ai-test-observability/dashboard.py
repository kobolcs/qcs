import streamlit as st
import demo_pandas as pd
import plotly.express as px
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.cluster import KMeans

# Set the layout of the Streamlit page
st.set_page_config(layout="wide")

st.title("🤖 AI-Powered Test Observability Dashboard")

# File uploader allows for dynamic analysis
uploaded_file = st.file_uploader("Upload your processed_results.csv file", type=["csv"])

if uploaded_file is not None:
    try:
        df = pd.read_csv(uploaded_file)
    except Exception as e:
        st.error(f"Error reading the CSV file: {e}")
        st.stop()

    # --- Basic Metrics ---
    st.header("📊 High-Level Test Suite Metrics")
    total_tests = len(df)
    passed_count = df[df['status'] == 'passed'].shape[0]
    failed_count = df[df['status'] == 'failed'].shape[0]

    if total_tests > 0:
        pass_rate = (passed_count / total_tests) * 100
    else:
        pass_rate = 0

    col1, col2, col3 = st.columns(3)
    col1.metric("Total Tests", total_tests)
    col2.metric("Pass Rate", f"{pass_rate:.2f}%")
    col3.metric("Total Failures", failed_count)

    # --- Failure Analysis ---
    st.header("🔬 Failure Analysis")
    failed_tests = df[df['status'] == 'failed'].copy()

    if not failed_tests.empty:
        # 1. NLP Clustering for Root Cause Analysis
        st.subheader("Clustered Failure Reasons")

        # Fill NaN values in failure messages to avoid errors
        failed_tests['failure_message'] = failed_tests['failure_message'].fillna('No error message')

        # Use TF-IDF to convert text messages into numerical vectors
        vectorizer = TfidfVectorizer(stop_words='english', max_df=0.5)
        X = vectorizer.fit_transform(failed_tests['failure_message'])

        # Use KMeans to cluster the failure messages
        # We aim for a small number of clusters to group common root causes
        num_clusters = min(5, len(failed_tests['failure_message'].unique()))
        if num_clusters > 0:
            kmeans = KMeans(n_clusters=num_clusters, random_state=42, n_init='auto')
            failed_tests['cluster'] = kmeans.fit_predict(X)

            # Display the clusters
            for i in range(num_clusters):
                cluster_data = failed_tests[failed_tests['cluster'] == i]
                # Try to find a representative error message for the cluster title
                common_message = cluster_data['failure_message'].mode().iloc[0]
                with st.expander(f"Cluster {i+1}: {common_message[:80]}... ({len(cluster_data)} failures)"):
                    st.dataframe(cluster_data[['name', 'failure_message']])
        else:
            st.write("Not enough unique failure messages to perform clustering.")

    else:
        st.success("No failures to analyze!")

    # --- Performance and Flakiness ---
    st.header("⏱️ Performance & Flakiness")
    col1, col2 = st.columns(2)

    with col1:
        # 2. Slowest Tests
        st.subheader("Top 5 Slowest Tests")
        slowest_tests = df.sort_values(by='time', ascending=False).head(5)
        st.dataframe(slowest_tests[['name', 'time']],
                     column_config={"time": st.column_config.NumberColumn(format="%.2fs")})

    with col2:
        # 3. Flaky Test Detection
        st.subheader("Potentially Flaky Tests")
        flaky_df = df[df['is_flaky'] == True]
        if not flaky_df.empty:
            # Show only the unique names of flaky tests
            st.dataframe(flaky_df[['name']].drop_duplicates())
        else:
            st.info("No flaky tests detected in this dataset.")
else:
    st.info("Please upload a CSV file to begin analysis.")

